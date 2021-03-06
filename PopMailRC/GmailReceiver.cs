﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Net;
using System.Threading;
using System.Web;

namespace MailExposure.PopMailRC
{
    class GmailReceiver:MailStr
    {
        // Fields
        private CookieContainer cookieContainer;
        public string m_emailTime;
        private int page;
        private string urls;
        private string urlTemp;

        // Methods
        public GmailReceiver()
        {
            this.urls = "";
            this.urlTemp = "";
        }
        private void DownEmail(string emailBox)
        {
            string str = "";
            string url = "";
            int startIndex = 0;
            int[] numArray = new int[200];
            if (emailBox == "收件箱")
            {
                str = "?st=";
                url = base.Host + "?";
            }
            else if (emailBox == "已删除邮件")
            {
                str = "?s=t&st=";
                url = base.Host + "?s=t";
            }
            else if (emailBox == "垃圾邮件")
            {
                str = "?s=m&st=";
                url = base.Host + "?s=m";
            }
            else if (emailBox == "已发邮件")
            {
                str = "?s=s&st=";
                url = base.Host + "?s=s";
            }
            else if (emailBox == "草稿箱")
            {
                str = "?s=d&st=";
                url = base.Host + "?s=d";
            }
            base.MyStringBuilder.Remove(0, base.MyStringBuilder.Length);
            base.streamControl = true;
            base.MyStringBuilder = this.Request(url);
            this.page = 0;
            this.page++;
            base.ShowMessage(string.Concat(new object[] { base.BoxName, "：第", this.page, "页开始下载" }));
            this.getEmailId(base.MyStringBuilder.ToString());
            base.ShowMessage(string.Concat(new object[] { base.BoxName, "：第", this.page, "页下载完毕" }));
            while ((startIndex = base.MyStringBuilder.ToString().IndexOf(str, startIndex)) > 0)
            {
                string str3 = base.putstr(base.MyStringBuilder.ToString(), "=", "\"", startIndex).Trim();
                if (str3 == "-1")
                {
                    startIndex++;
                }
                else
                {
                    if (str3.IndexOf("=") > -1)
                    {
                        str3 = str3.Substring(str3.IndexOf("=") + 1, str3.Length - (str3.IndexOf("=") + 1));
                    }
                    numArray[this.page] = Convert.ToInt32(str3);
                    if (numArray[this.page] > numArray[this.page - 1])
                    {
                        url = base.Host + str + numArray[this.page];
                        base.MyStringBuilder.Remove(0, base.MyStringBuilder.Length);
                        base.streamControl = true;
                        base.MyStringBuilder = this.Request(url);
                        this.page++;
                        if (Convert.ToInt32(GlobalValue.PopMainForm.PageNumber) != 0)
                        {
                            if (this.page >= Convert.ToInt32(GlobalValue.PopMainForm.PageNumber))
                            {
                                return;
                            }
                            base.ShowMessage(string.Concat(new object[] { base.BoxName, "：第", this.page, "页开始下载" }));
                            this.getEmailId(base.MyStringBuilder.ToString());
                            base.ShowMessage(string.Concat(new object[] { base.BoxName, "：第", this.page, "页下载完毕" }));
                            startIndex = 0;
                        }
                        else
                        {
                            base.ShowMessage(string.Concat(new object[] { base.BoxName, "：第", this.page, "页开始下载" }));
                            this.getEmailId(base.MyStringBuilder.ToString());
                            base.ShowMessage(string.Concat(new object[] { base.BoxName, "：第", this.page, "页下载完毕" }));
                            startIndex = 0;
                        }
                        continue;
                    }
                    startIndex++;
                }
            }

        }
        private void getEmailId(string myMessage)
        {
            string str;
            base.emailInfo = null;
            base.emailInfo = new MailStr.EmailInfo_Gmail[200];
            int startIndex = 0;
            int index = 0;
        Label_001B:
            str = "";
            int num3 = myMessage.IndexOf("checkbox", startIndex);
            if (num3 < 0)
            {
                return;
            }
            str = base.putstr(myMessage, "value=\"", "\"", num3);
            if (str == "-1")
            {
                startIndex++;
                goto Label_001B;
            }
            base.emailInfo[index].EmailID = str;
            str = "";
            str = base.putstr(myMessage, "href=\"", "\"", num3).Replace("v=c", "v=om");
            base.emailInfo[index].EmailUrl = str;
            string strSql = "select count(*) from GmailId where MsgId='" + base.emailInfo[index].EmailID + "'";
            if (Convert.ToInt32(GlobalValue.PopMainForm.ExecuteSQL(strSql)) == 0)
            {
                string str3 = DateTime.Now.ToString();
                base.streamControl = true;
                string[] strArray = new string[] { base.BoxName, "第", (index + 1).ToString("00"), "封:", base.emailInfo[index].EmailID, " 开始下载" };
                base.ShowMessage(string.Concat(strArray));
                try
                {
                    int num7;
                    if (!this.RequestEmail(base.Host + base.emailInfo[index].EmailUrl))
                    {
                        goto Label_043F;
                    }
                    int num5 = 0;
                    if (!(base.BoxName == "草稿箱"))
                    {
                        goto Label_0394;
                    }
                    int num6 = 0;
                    base.MyStringBuilder.Remove(0, base.MyStringBuilder.Length);
                    base.streamControl = true;
                    base.MyStringBuilder = this.Request(base.Host + base.emailInfo[index].EmailUrl);
                Label_01ED:
                    num7 = base.MyStringBuilder.ToString().IndexOf("checkbox", num5);
                    if (num7 >= 0)
                    {
                        string str4 = base.putstr(base.MyStringBuilder.ToString(), "value=\"", "\"", num7);
                        if (str4 == "-1")
                        {
                            num5++;
                        }
                        else
                        {
                            string str5 = str4.Substring(0, str4.IndexOf("_", 5) - 1);
                            string str6 = str4.Substring(str4.IndexOf("_", 5) + 1, (str4.Length - str4.IndexOf("_", 5)) - 1);
                            str4 = "&realattid=" + str5 + "&attid=" + str6 + "&disp=attd&view=att";
                            string str7 = base.putstr(base.MyStringBuilder.ToString(), "<b>", "</b>", num7);
                            if (str7 == "-1")
                            {
                                num5++;
                            }
                            else
                            {
                                num6++;
                                base.AttId = "附件";
                                base.AttName = string.Concat(new object[] { base.AttId, "-", num6, "_", str7 });
                                string url = base.Host + base.emailInfo[index].EmailUrl + str4;
                                base.streamControl = true;
                                this.RequestEmail(url);
                                base.AttId = "";
                                num5 = num7 + 1;
                            }
                        }
                        goto Label_01ED;
                    }
                Label_0394:
                    try
                    {
                        strSql = "insert into GmailId(Name,MsgId,DownTime,MailType) values('" + base.m_username + "','" + base.emailInfo[index].EmailID + "','" + str3 + "','" + base.mailType + "')";
                        GlobalValue.PopMainForm.ExecuteSQL(strSql);
                    }
                    catch (Exception exception)
                    {
                        base.ShowMessage("添加失败：" + exception.Message);
                    }
                }
                catch (Exception exception2)
                {
                    base.ShowMessage(exception2.Message);
                }
            }
        Label_043F:
            startIndex = num3 + 1;
            index++;
            goto Label_001B;

        }
        private string getGmailCookie(string strCookie)
        {
            string str = "";
            string str2 = "";
            strCookie = " " + strCookie + "; ";
            str = base.putstr(strCookie, " SID=", " ", 0);
            if (str != "-1")
            {
                str2 = str2 + "SID=" + str + " ";
            }
            str = base.putstr(strCookie, " HSID=", " ", 0);
            if (str != "-1")
            {
                str2 = str2 + "HSID=" + str + " ";
            }
            str = base.putstr(strCookie, " SSID=", " ", 0);
            if (str != "-1")
            {
                str2 = str2 + "SSID=" + str + " ";
            }
            return str2;

        }
        private StringBuilder getLocation(string message)
        {
            int startIndex = 0;
            startIndex = message.IndexOf("location.replace(");
            if (startIndex > 0)
            {
                string url = base.putstr(message, "(\"", "\")", startIndex);
                if (url != "-1")
                {
                    base.MyStringBuilder.Remove(0, base.MyStringBuilder.Length);
                    base.streamControl = true;
                    base.MyStringBuilder = this.Request(url);
                    string str2 = base.MyStringBuilder.ToString();
                    this.getLocation(str2);
                }
            }
            return base.MyStringBuilder;

        }
        public override void login()
        {
            base.m_passwd = base.strPassParse(base.m_passwd);
            try
            {
                int startIndex = 0;
                string url = "";
                string indata = "";
                base.cookie = "";
                this.cookieContainer = null;
                this.cookieContainer = new CookieContainer();
                base.ShowMessage("开始登陆…………");
                url = "http://mail.google.com/mail/";
                base.MyStringBuilder.Remove(0, base.MyStringBuilder.Length);
                base.streamControl = true;
                base.MyStringBuilder = this.Request(url);
                this.urlTemp = this.urls;
                string str3 = "";
                startIndex = base.MyStringBuilder.ToString().IndexOf("GALX");
                if (startIndex > 0)
                {
                    str3 = base.putstr(base.MyStringBuilder.ToString(), "value=\"", "\"", startIndex);
                }
                url = "https://www.google.com/accounts/ServiceLoginAuth?service=mail";
                indata = "ltmpl=default&ltmplcache=2&continue=http%3A%2F%2Fmail.google.com%2Fmail%2F%3F&service=mail&rm=false&ltmpl=default&ltmpl=default&scc=1&GALX=" + str3 + "&Email=" + base.m_username + "&Passwd=" + base.m_passwd + "&rmShown=1&signIn=%E7%99%BB%E5%BD%95&asts=";
                base.MyStringBuilder.Remove(0, base.MyStringBuilder.Length);
                base.streamControl = true;
                base.MyStringBuilder = this.PostData(url, indata, this.cookieContainer);
                if (base.MyStringBuilder.ToString().IndexOf("errormsg_0_Passwd") != -1)
                {
                    base.LoginFail();
                    base.passwdErr();
                    base.ShowMessage("用户名或密码不正确");
                    base.ShowMessage("登陆失败…………");
                }
                else
                {
                    startIndex = base.MyStringBuilder.ToString().IndexOf("<A HREF=\"");
                    if (startIndex > 0)
                    {
                        url = base.putstr(base.MyStringBuilder.ToString(), "<A HREF=\"", "\"", startIndex).Replace("&amp;", "&").Replace("&amp;", "&");
                        if (url != "-1")
                        {
                            base.MyStringBuilder.Remove(0, base.MyStringBuilder.Length);
                            base.streamControl = true;
                            base.MyStringBuilder = this.Request(url);
                        }
                    }
                    startIndex = base.MyStringBuilder.ToString().IndexOf("location.replace(");
                    if (startIndex > 0)
                    {
                        url = base.putstr(base.MyStringBuilder.ToString(), "(\"", "\")", startIndex);
                        if (url != "-1")
                        {
                            url = url.Replace(@"\x3d", "=").Replace(@"\x26", "&");
                            base.MyStringBuilder.Remove(0, base.MyStringBuilder.Length);
                            base.streamControl = true;
                            base.MyStringBuilder = this.Request(url);
                        }
                    }
                    startIndex = base.MyStringBuilder.ToString().IndexOf("<A HREF=\"");
                    if (startIndex > 0)
                    {
                        url = base.putstr(base.MyStringBuilder.ToString(), "<A HREF=\"", "\"", startIndex).Replace("&amp;", "&").Replace("&amp;", "&");
                        if (url != "-1")
                        {
                            base.MyStringBuilder.Remove(0, base.MyStringBuilder.Length);
                            base.streamControl = true;
                            base.MyStringBuilder = this.Request(url);
                        }
                    }
                    int index = base.MyStringBuilder.ToString().IndexOf("<base");
                    if (index < 0)
                    {
                        string str4 = "";
                        if (base.MyStringBuilder.ToString().IndexOf("<html lang=\"zh-CN\">") != -1)
                        {
                            str4 = "http://mail.google.com/mail/?ui=html&amp;zy=d";
                        }
                        else
                        {
                            str4 = "http://mail.google.com/mail/?ui=html&amp;zy=e";
                        }
                        base.MyStringBuilder.Remove(0, base.MyStringBuilder.Length);
                        base.streamControl = true;
                        base.MyStringBuilder = this.Request(str4);
                        startIndex = base.MyStringBuilder.ToString().IndexOf("<A HREF=\"");
                        if (startIndex > 0)
                        {
                            url = base.putstr(base.MyStringBuilder.ToString(), "<A HREF=\"", "\"", startIndex).Replace("&amp;", "&").Replace("&amp;", "&");
                            if (url != "-1")
                            {
                                base.MyStringBuilder.Remove(0, base.MyStringBuilder.Length);
                                base.streamControl = true;
                                base.MyStringBuilder = this.Request(url);
                            }
                        }
                        index = base.MyStringBuilder.ToString().IndexOf("<base");
                        if (index < 0)
                        {
                            base.LoginFail();
                            base.passwdErr();
                            base.ShowMessage("登陆失败…………");
                            return;
                        }
                    }
                    base.Host = base.putstr(base.MyStringBuilder.ToString(), "\"", "\"", index);
                    if (base.Host == "-1")
                    {
                        base.LoginFail();
                        base.passwdErr();
                        base.ShowMessage("登陆失败…………");
                    }
                    else if (base.Host == null)
                    {
                        base.LoginFail();
                        base.passwdErr();
                        base.ShowMessage("登陆失败…………");
                    }
                    else
                    {
                        base.ShowMessage("登陆成功！");
                        url = "https://mail.google.com/mail/contacts/data/export?groupToExport=%5EMine&exportType=ALL&out=GMAIL_CSV";
                        base.MyStringBuilder.Remove(0, base.MyStringBuilder.Length);
                        base.streamControl = true;
                        base.MyStringBuilder = this.Request(url);
                        if (base.MyStringBuilder.ToString() != "")
                        {
                            this.SaveAddressbook(base.MyStringBuilder.ToString());
                        }
                        this.StartReceivce();
                    }
                }
            }
            catch (Exception exception)
            {
                base.LoginFail();
                base.passwdErr();
                base.ShowMessage("登陆失败…………" + exception.Message);
            }

        }
        public StringBuilder PostData(string url, string indata, CookieContainer cookieContainer)
        {
            try
            {
                StreamReader reader;
                base.cookie = base.cookie.Replace("; ;", ";").Trim(new char[] { ',' });
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                request.ServicePoint.Expect100Continue = false;
                request.KeepAlive = true;
                request.Method = "POST";
                request.Accept = "image/gif, image/jpeg, image/pjpeg, image/pjpeg, application/x-shockwave-flash, application/msword, application/vnd.ms-excel, application/vnd.ms-powerpoint, application/x-ms-application, application/x-ms-xbap, application/vnd.ms-xpsdocument, application/xaml+xml, */*";
                request.Headers.Add("Accept-Language: zh-cn");
                request.ContentType = "application/x-www-form-urlencoded";
                request.UserAgent = "Mozilla/4.0 (compatible; MSIE 8.0; Windows NT 5.1; Trident/4.0; .NET CLR 2.0.50727; .NET CLR 3.0.04506.648; .NET CLR 3.5.21022)";
                request.ContentLength = indata.Length;
                request.AllowAutoRedirect = false;
                request.Referer = this.urlTemp;
                request.Headers.Add("Cookie", base.cookie.Trim(new char[] { ',' }));
                Stream requestStream = request.GetRequestStream();
                char[] chars = Encoding.ASCII.GetChars(Encoding.ASCII.GetBytes(indata));
                StreamWriter writer = new StreamWriter(requestStream);
                writer.Write(chars, 0, chars.Length);
                writer.Close();
                requestStream.Close();
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                Thread.Sleep(10);
                response.GetResponseStream();
                WebHeaderCollection headers = response.Headers;
                if (response.Headers["Set-Cookie"] != null)
                {
                    base.cookie = base.cook(response.Headers["Set-Cookie"]);
                }
                string str = "";
                if (headers["Content-Type"] != null)
                {
                    str = headers["Content-Type"];
                    if (str.IndexOf("charset=") >= 0)
                    {
                        base.charSet = str.Substring(str.IndexOf("charset=") + 8, (str.Length - str.IndexOf("charset=")) - 8);
                    }
                }
                if (((response.StatusCode == HttpStatusCode.Found) || (response.StatusCode == HttpStatusCode.MovedPermanently)) || ((response.StatusCode == HttpStatusCode.MovedPermanently) || (response.StatusCode == HttpStatusCode.Found)))
                {
                    this.urls = headers["location"];
                    if (this.urls.IndexOf("http") != -1)
                    {
                        base.MyStringBuilder = this.Request(this.urls);
                    }
                }
                if ((base.charSet == null) || (base.charSet == "-1"))
                {
                    reader = new StreamReader(response.GetResponseStream(), Encoding.Default);
                }
                else
                {
                    reader = new StreamReader(response.GetResponseStream(), Encoding.GetEncoding(base.charSet));
                }
                if (base.streamControl)
                {
                    base.MyStringBuilder.Remove(0, base.MyStringBuilder.Length);
                    for (string str2 = reader.ReadLine(); str2 != null; str2 = reader.ReadLine())
                    {
                        base.MyStringBuilder.Append(str2 + "\r\n");
                        str2 = null;
                    }
                }
                response.Close();
                response = null;
                base.streamControl = false;
                reader.Close();
                reader = null;
            }
            catch (Exception exception)
            {
                base.ShowMessage("Gpost:" + exception.Message);
            }
            return base.MyStringBuilder;

        }
        public StringBuilder Request(string url)
        {
            url.IndexOf("https://mail.google.com/mail/?ui");
            if (url.IndexOf("https://mail.google.com/mail/?pli=1&auth=") != -1)
            {
                string str = this.getGmailCookie(base.cookie);
                if (str != "")
                {
                    base.cookie = str;
                }
            }
            if (url.IndexOf("https://mail.google.com/mail/?auth=") != -1)
            {
                string str2 = this.getGmailCookie(base.cookie);
                if (str2 != "")
                {
                    base.cookie = str2;
                }
            }
            try
            {
                StreamReader reader;
                base.cookie = base.cookie.Replace("; ;", ";").Trim(new char[] { ',' });
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                request.KeepAlive = true;
                if (url.IndexOf("https://mail.google.com/mail/?auth=") != -1)
                {
                    request.Referer = this.urlTemp;
                }
                if (url.IndexOf("http://www.google.com.tw/accounts/SetSID?ssdc=1") < 0)
                {
                    request.Headers.Add("Cookie", base.cookie.Trim(new char[] { ',' }));
                }
                request.Method = "GET";
                request.Accept = "image/gif, image/jpeg, image/pjpeg, image/pjpeg, application/x-shockwave-flash, application/msword, application/vnd.ms-excel, application/vnd.ms-powerpoint, application/x-ms-application, application/x-ms-xbap, application/vnd.ms-xpsdocument, application/xaml+xml, */*";
                request.AllowAutoRedirect = false;
                request.UserAgent = "Mozilla/4.0 (compatible; MSIE 8.0; Windows NT 5.1; Trident/4.0; .NET CLR 2.0.50727; .NET CLR 3.0.04506.648; .NET CLR 3.5.21022)";
                request.Headers.Add("Accept-Language: zh-cn");
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                Thread.Sleep(10);
                WebHeaderCollection headers = response.Headers;
                if ((url.IndexOf("/accounts/SetSID?ssdc=1") < 0) && (response.Headers["Set-Cookie"] != null))
                {
                    base.cookie = base.cook(response.Headers["Set-Cookie"]);
                }
                if (headers["Content-Type"] != null)
                {
                    string str3 = "";
                    str3 = headers["Content-Type"];
                    if (str3.IndexOf("charset=") >= 0)
                    {
                        base.charSet = str3.Substring(str3.IndexOf("charset=") + 8, (str3.Length - str3.IndexOf("charset=")) - 8);
                    }
                }
                if (((response.StatusCode == HttpStatusCode.Found) || (response.StatusCode == HttpStatusCode.MovedPermanently)) || ((response.StatusCode == HttpStatusCode.MovedPermanently) || (response.StatusCode == HttpStatusCode.Found)))
                {
                    this.urls = headers["location"];
                    if (this.urls.IndexOf("http") != -1)
                    {
                        this.Request(this.urls);
                    }
                }
                if ((base.charSet == null) || (base.charSet == "-1"))
                {
                    reader = new StreamReader(response.GetResponseStream(), Encoding.Default);
                }
                else
                {
                    reader = new StreamReader(response.GetResponseStream(), Encoding.GetEncoding(base.charSet));
                }
                if (base.streamControl)
                {
                    base.MyStringBuilder.Remove(0, base.MyStringBuilder.Length);
                    for (string str4 = reader.ReadLine(); str4 != null; str4 = reader.ReadLine())
                    {
                        base.MyStringBuilder.Append(str4 + "\r\n");
                        str4 = null;
                    }
                }
                response.Close();
                response = null;
                base.streamControl = false;
                reader.Close();
                reader = null;
            }
            catch (Exception exception)
            {
                base.ShowMessage("Grequest:" + exception.Message);
            }
            return base.MyStringBuilder;

        }
        public bool RequestEmail(string url)
        {
            int num;
            base.cookie = base.cookie.Replace("; ;", ";");
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.Headers.Add("Cookie", base.cookie);
            request.Method = "GET";
            request.Accept = "*/*";
            request.AllowAutoRedirect = false;
            request.UserAgent = "Mozilla/4.0 (compatible; MSIE 7.0; Windows NT 5.1; SV1)";
            request.ContentType = "application/x-www-form-urlencoded";
            request.Headers.Add("Accept-Language: zh-cn");
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            Thread.Sleep(10);
            Stream responseStream = response.GetResponseStream();
            if (!base.streamControl)
            {
                return false;
            }
            byte[] buffer = new byte[0x400];
            base.m_emailno++;
            this.m_emailTime = DateTime.Now.ToString("yyyMMdd-HHmmss") + DateTime.Now.Millisecond.ToString();
            while ((num = responseStream.Read(buffer, 0, 0x400)) > 0)
            {
                if (!this.SaveGmail(buffer, num))
                {
                    return false;
                }
            }
            base.saveEmailCount();
            GlobalValue.PopMainForm.statusBarPanelEmailNumbersText.Text = (Convert.ToInt32(GlobalValue.PopMainForm.statusBarPanelEmailNumbersText.Text) + 1).ToString();
            base.streamControl = false;
            responseStream.Close();
            return true;

        }
        public void SaveAddressbook(string EmailText)
        {
            string path = "";
            if ((GlobalValue.PopMainForm.saveFilePath != "") | (GlobalValue.PopMainForm.saveFilePath != null))
            {
                path = GlobalValue.PopMainForm.saveFilePath;
            }
            string str2 = DateTime.Now.Date.ToString("yyy-MM-dd");
            path = path + str2 + "邮件";
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            path = path + @"\" + base.m_snote;
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            path = path + @"\" + base.m_stype;
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            string str3 = path;
            path = str3 + @"\" + base.m_username + "@" + base.m_serv;
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            path = path + @"\地址薄";
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            Stream stream = File.Create(path + @"\" + base.m_username + "-contacts.csv");
            using (StreamWriter writer = new StreamWriter(stream, Encoding.Default))
            {
                writer.Write(EmailText);
            }
            stream.Close();
            stream = null;

        }
        public bool SaveGmail(byte[] buffer, int nbytes)
        {
            try
            {
                string filePath = "";
                if ((GlobalValue.PopMainForm.saveFilePath != "") | (GlobalValue.PopMainForm.saveFilePath != null))
                {
                    filePath = GlobalValue.PopMainForm.saveFilePath;
                }
                this.SaveGmailText(buffer, nbytes, filePath);
            }
            catch (Exception exception)
            {
                base.ShowMessage("保存邮件失败：" + exception.Message);
                return false;
            }
            return true;

        }
        public void SaveGmailText(byte[] buffer, int nbytes, string filePath)
        {
            FileStream stream = null;
            try
            {
                string attName = ".eml";
                if (base.BoxName == "草稿箱")
                {
                    attName = ".html";
                }
                if ((base.BoxName == "草稿箱") && (base.AttId == "附件"))
                {
                    attName = base.AttName;
                }
                if (filePath == null)
                {
                    filePath = "";
                }
                string str2 = DateTime.Now.Date.ToString("yyy-MM-dd");
                filePath = filePath + str2 + "邮件";
                if (!Directory.Exists(filePath))
                {
                    Directory.CreateDirectory(filePath);
                }
                filePath = filePath + @"\" + base.m_snote;
                if (!Directory.Exists(filePath))
                {
                    Directory.CreateDirectory(filePath);
                }
                filePath = filePath + @"\" + base.m_stype;
                if (!Directory.Exists(filePath))
                {
                    Directory.CreateDirectory(filePath);
                }
                string str3 = filePath;
                filePath = str3 + @"\" + base.m_username + "@" + base.m_serv;
                if (!Directory.Exists(filePath))
                {
                    Directory.CreateDirectory(filePath);
                }
                filePath = filePath + @"\" + base.BoxName;
                if (!Directory.Exists(filePath))
                {
                    Directory.CreateDirectory(filePath);
                }
                filePath = filePath + @"\" + this.m_emailTime + attName;
                stream = File.Open(filePath, FileMode.Append, FileAccess.Write);
                stream.Write(buffer, 0, nbytes);
                stream.Close();
            }
            catch (Exception exception)
            {
                base.ShowMessage("保存" + exception.Message);
            }
            finally
            {
                stream.Close();
            }

        }
        private void StartReceivce()
        {
            try
            {
                base.BoxName = "收件箱";
                this.DownEmail(base.BoxName);
                base.BoxName = "已删除邮件";
                this.DownEmail(base.BoxName);
                base.BoxName = "垃圾邮件";
                this.DownEmail(base.BoxName);
                base.BoxName = "已发邮件";
                this.DownEmail(base.BoxName);
                base.BoxName = "草稿箱";
                this.DownEmail(base.BoxName);
            }
            catch (Exception exception)
            {
                base.ShowMessage(exception.Message);
            }
        }
    }
}
