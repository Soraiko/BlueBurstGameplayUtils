using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BlueBurstGameplayUtils
{
    public class SrkSettings
    {
        string filename;
        string separator;

        List<string> keys;
        List<string> values;

        public SrkSettings(string filename, string[] default_settings, string separator)
        {
            this.filename = filename;
            this.separator = separator;
            this.keys = new List<string>(0);
            this.values = new List<string>(0);

            if (File.Exists(filename))
            {
                StringArrayToKeysValues(File.ReadAllLines(filename), ref this.keys, ref this.values, this.separator);
            }
            else
            {
                File.WriteAllLines(this.filename, default_settings);
                StringArrayToKeysValues(default_settings, ref this.keys, ref this.values, this.separator);
            }
        }

        static void StringArrayToKeysValues(string[] array, ref List<string> keys, ref List<string> values, string separator)
        {
            foreach (string element in array)
            {
                string[] split = element.Split(separator[0]);
                int index = keys.IndexOf(split[0]);
                if (index < 0)
                {
                    keys.Add(split[0]);
                    values.Add(split[1]);
                }
                else
                {
                    keys[index] = split[1];
                }
            }
        }

        static string[] KeysValuesToStringArray(ref List<string> keys, ref List<string> values, string separator)
        {
            string[] ouput = new string[keys.Count];
            for (int i=0;i<keys.Count;i++)
            {
                ouput[i] = keys[i] + separator + values[i];
            }
            return ouput;
        }

        public string GetProperty(string key)
        {
            string output = "";
            int index = this.keys.IndexOf(key);
            if (index>-1)
            {
                output = this.values[index];
            }
            return output;
        }

        public void SetProperty(string key, string value)
        {
            int index = this.keys.IndexOf(key);
            if (index > -1)
            {
                this.values[index] = value;
            }
            else
            {
                this.keys.Add(key);
                this.values.Add(value);
            }
            File.WriteAllLines(this.filename, KeysValuesToStringArray(ref this.keys, ref this.values, this.separator));
        }

        public SrkSettings(string filename, string separator) : this(filename, new string[0], separator)
        {

        }
    }
}
