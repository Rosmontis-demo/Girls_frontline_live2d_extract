using System;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;


namespace CubismFadeMotionDataToJson
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("CubismFadeMontionDataToJson 版本0.1b1");
            Console.WriteLine("功能：用UABE以json形式从assess bundle导出cubismfademotiondata类的数据，可以用此程序转换为一般的live2d动作文件");
            Console.WriteLine("仅支持cubism live2d sdk3 motion json的导出");
            Console.WriteLine("警告：处于实验性阶段，出现问题很正常");
            Console.WriteLine("按任意键开始");
            Console.ReadKey();

            if (Directory.Exists("dst"))
            {
                string[] existFilename = Directory.GetFiles("dst");
                foreach (string i in existFilename)
                    File.Delete(i);
            }
            else
            {
                Directory.CreateDirectory("dst");
            }
            string[] filenames = Directory.GetFiles("src");
            MotionDataConverter converter = new MotionDataConverter();
            foreach (string i in filenames)
            {
                Console.WriteLine(string.Format("转换：[0]......", i));
                try
                {

                    var json = File.ReadAllText(i);
                    var infinity = JsonConvert.PositiveInfinity;
                    var newContent = json.Replace("1.#INF", infinity.ToString());
                    var obj = JObject.Parse(newContent);
                    var content = converter.Convert(obj).ToString();
                    File.WriteAllText("dst/" + Path.GetFileName(i),content);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                    Console.WriteLine("转换失败");

                }
                
            }
            Console.WriteLine("全部转换成功，现在开始改名");
            RenameMacro();
            Console.WriteLine("successful");
            Console.ReadKey();

        }
        static void RenameMacro()
        {
            string[] filenames = Directory.GetFiles("dst");
            for (int i = 0; i < filenames.Length; ++i)
            {
                string name = Path.GetFileName(filenames[i]);
                int chrPos = name.LastIndexOf('-'); // find last '-'
                if (chrPos == -1)
                    continue;
                name = name.Remove(chrPos,name.Length - chrPos);
                name += ".motion3.json";
                File.Move(filenames[i], "dst/" + name);

            }
        }
    }
}
