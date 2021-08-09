using System;
using System.Net;
using System.IO;

namespace an631{
    class Program{
		static bool err=false;
		static string token,linkSend,linkAsk,linkUpd,key,id;
		static string[] kw=new string[100],kwA=new string[100],awA=new string[100],aw=new string[100];//ключевые слова и возможные ответы
		static int Nkw,NkwA,Naw,NawA;//KeyWord; AnsWer; Agressive
		static Random rand=new Random();
		static StreamReader fr;
		static StreamWriter fw;

		static string find(string json,string key,bool isint=false){//поиск значения по ключу в json записанным строкой
			int a,b;
			if(isint){
				a=json.IndexOf("\""+key+"\":")+key.Length+3;
				string d=json.Substring(a,json.Length-a);
				b=d.IndexOf(",\"");
			}else{
				a=json.IndexOf("\""+key+"\":\"")+key.Length+4;
				string d=json.Substring(a,json.Length-a);
				b=d.IndexOf("\",\"");
			}
		return json.Substring(a,b);
		}

		static string request(string link){//сетевой запрос с получением json ответа строкой
			WebRequest request=WebRequest.Create(link);
			request.Credentials=CredentialCache.DefaultCredentials;
			HttpWebResponse response=(HttpWebResponse)request.GetResponse();
			Stream data=response.GetResponseStream();
			StreamReader reader=new StreamReader(data);
			string r=reader.ReadToEnd();
		reader.Close();
		data.Close();
		response.Close();
		return r;
		}

		static bool sendMsg(string peer,string text){
			Console.Write("'");
			string answ=request(linkSend+peer+"&message="+text);
		return true;
		}

		static void getKey(){
			key=find(request(linkAsk),"key");
			linkUpd="https://lp.vk.com/wh"+id+"?act=a_check&key="+key+"&wait=60&mode=2&ts=";
		}

		static string list(string[]ss,int n){
			string l="";
			for(int i=1;i<=n;++i)
				l+=i+")"+ss[i-1]+"\n";
		return l;
		}

		static void addd(ref string[]ss,ref int n,string source,string destination){
			ss[n]=source;
			fw=new StreamWriter(destination,true);
			fw.WriteLine(ss[n]);
			++n;
		fw.Close();
		}

		static void delete(ref string[]ss,ref int n,int k,string destination){
			for(int i=k-1;i<n;++i)
				ss[i]=ss[i+1];
			--n;
			fw=new StreamWriter(destination);
			for(int i=0;i<n;++i)
				fw.WriteLine(ss[i]);
		fw.Close();
		}

		static bool check(int ts){
			string answer=request(linkUpd+ts);
			if(answer.Length==(24+ts.ToString().Length)){
				return false;//пропуск цикла в main, чтобы не увеличивать TS, когда событий нет
			}
			if(answer.Contains("failed")){//допили потом 
				Console.WriteLine("ERROR");
				Console.Write(answer);
				if(answer[10]=='2'){//непроверенный элемент
					Console.WriteLine("попытка получить новый KEY");
					getKey();
					Console.WriteLine("DONE");
				}else{
					err=true;
				}
				return false;
			}
			string msg=find(answer,"text");
			if(msg.Length>1){//кринж ,а не дерево, зато нет выхода за границы массива
				if(msg[1]=='/'){
					if(msg.Length>3){
						if(msg[2]=='l'){
							if(msg[4]=='0')
								if(msg[3]=='a')
									sendMsg(find(answer,"peer_id",true),list(kwA,NkwA));
								else
									sendMsg(find(answer,"peer_id",true),list(kw,Nkw));
							if(msg[4]=='1')
								if(msg[3]=='a')
									sendMsg(find(answer,"peer_id",true),list(awA,NawA));
								else
									sendMsg(find(answer,"peer_id",true),list(aw,Naw));
						}else{
							if(msg.Length>6){
								if(msg[2]=='a'){
									if(msg[4]=='0')
										if(msg[3]=='a')
											addd(ref kwA,ref NkwA,msg.Substring(6,msg.Length-6),"keyWordsA.txt");
										else
											addd(ref kw,ref Nkw,msg.Substring(6,msg.Length-6),"keyWords.txt");
									if(msg[4]=='1')
										if(msg[3]=='a')
											addd(ref awA,ref NawA,msg.Substring(6,msg.Length-6),"answersA.txt");
										else
											addd(ref aw,ref Naw,msg.Substring(6,msg.Length-6),"answers.txt");
								}else{
									if(msg[2]=='d'){
										if(msg[4]=='0')
											if(msg[3]=='a')
												delete(ref kwA,ref NkwA,int.Parse(msg.Substring(6,msg.Length-6)),"keyWordsA.txt");
											else
												delete(ref kw,ref Nkw,int.Parse(msg.Substring(6,msg.Length-6)),"keyWords.txt");
										if(msg[4]=='1')
											if(msg[3]=='a')
												delete(ref awA,ref NawA,int.Parse(msg.Substring(6,msg.Length-6)),"answersA.txt");
											else
												delete(ref aw,ref Naw,int.Parse(msg.Substring(6,msg.Length-6)),"answers.txt");
									}
								}
							}
						}
					}
				}else{
					for(int i=0;i<Nkw;++i)
						if(msg.Contains(kw[i]))
							sendMsg(find(answer,"peer_id",true),aw[rand.Next()%Naw]);
					for(int i=0;i<NkwA;++i)
						if(msg.Contains(kwA[i]))
							sendMsg(find(answer,"peer_id",true),awA[rand.Next()%NawA]);
				}
			}
		return true;
		}

		static void uploadList(ref string[]ss,ref int n,string way){
			fr=new StreamReader(way);
			int i=0;
			while(!fr.EndOfStream){
				ss[i]=fr.ReadLine();
				++i;
			}
			n=i;
		fr.Close();
		}

        static void Main(string[] args){
			Console.WriteLine("startin");
			fr=new StreamReader("login.txt");//данные о боте
			id=fr.ReadLine();//в первой строке group_id
			token=fr.ReadLine();//во второй токен с достаточными уровнями доступа
		fr.Close();
			linkSend="https://api.vk.com/method/messages.send?access_token="+token+"&v=5.131&random_id=0&peer_id=";
			linkAsk="https://api.vk.com/method/groups.getLongPollServer?access_token="+token+"&group_id="+id+"&v=5.131";
			getKey();
			fr=new StreamReader("lasTS.txt");
			int TS=int.Parse(fr.ReadLine());
		fr.Close();
			uploadList(ref kw,ref Nkw,"keyWords.txt");
			uploadList(ref kwA,ref NkwA,"keyWordsA.txt");
			uploadList(ref aw,ref Naw,"answers.txt");
			uploadList(ref awA,ref NawA,"answersA.txt");
			while(true){
				if(check(TS)){
					fw=new StreamWriter("lasTS.txt");
					TS++;
					fw.WriteLine(TS);
				fw.Close();
				}
				Console.Write(".");
				if(err){
					Console.WriteLine("работа прекращена с ошибкой");
				break;
				}
			}
        }
    }
}