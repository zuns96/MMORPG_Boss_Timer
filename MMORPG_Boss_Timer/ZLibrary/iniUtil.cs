using System;
using System.Text;
using System.Runtime.InteropServices;

namespace ZLibrary
{
    public class iniUtil
    {
        private string iniPath;

        [DllImport("kernel32.dll")]
        // GetIniValue 를 위해
        private static extern int GetPrivateProfileString(String section, String key, String def, StringBuilder retVal, int size, String filePath);

        [DllImport("kernel32.dll")]
        // SetIniValue를 위해
        private static extern long WritePrivateProfileString(String section, String key, String val, String filePath);

        public iniUtil(string path)
        {
            this.iniPath = path;  //INI 파일 위치를 생성할때 인자로 넘겨 받음
        }

        // INI 값을 읽어 온다.
        public String GetIniValue(String Section, String Key)
        {
            StringBuilder temp = new StringBuilder(255);
            int i = GetPrivateProfileString(Section, Key, "", temp, 255, iniPath);
            return temp.ToString();
        }

        // INI 값을 셋팅
        public void SetIniValue(String Section, String Key, String Value)
        {
            WritePrivateProfileString(Section, Key, Value, iniPath);
        }
    }
}
