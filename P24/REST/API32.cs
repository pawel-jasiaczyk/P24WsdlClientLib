using System;
using System.Text;
using System.Net.Http;
using System.Collections.Generic;
using System.Security.Cryptography;

namespace P24.REST
{
    public delegate void MyEventHandler(object sender, MyEventArgs e);
    public class MyEventArgs : EventArgs
    {
        public string Response{ get; set;}
    }


    public class API32
    {
        #region Private Fields

        private string crc;
        private int merchantId;
        private int posId;

        #endregion

        #region Properties

        public P24Mode P24Mode{ get; set; }

        #endregion

        #region Events

        public event MyEventHandler Received;

        #endregion

        #region C'tors

        public API32 (int merchantId, string crc)
        {
            this.merchantId = merchantId;
            this.posId = merchantId;
            this.crc = crc;
        }

        #endregion

        #region Public Methods

        public async void TestConnection()
        {
            Dictionary<string, string> values = new Dictionary<string, string> ();
            values.Add ("p24_merchant_id", this.merchantId.ToString());
            values.Add ("p24_pos_id", this.posId.ToString());

            string url = string.Format ("{0}/testConnection", GenerateBaseUrl ());

            string sign = ComputeMD5 (new string[] { this.posId.ToString (), this.crc });

            values.Add ("p24_sign", sign);

            using (HttpClient client = new HttpClient ())
            {
                var content = new FormUrlEncodedContent (values);
                var response = await client.PostAsync (url, content);
                var responseString =  await response.Content.ReadAsStringAsync();
                if (this.Received != null)
                {
                    Received (this, new MyEventArgs{ Response = responseString.ToString () }); 
                }
            }
        }

        #endregion

        #region Private Methods

        private void SetResponse(string response)
        {
            
        }

        private string ComputeMD5(string[] values)
        {
            StringBuilder source = new StringBuilder ();
            for (int i = 0; i < values.Length; i++)
            {
                source.Append (values [i]);
                if (i < values.Length - 1)
                    source.Append ("|");
            }

            StringBuilder result = new StringBuilder ();

            using (MD5 md5Hash = MD5.Create ())
            {
                byte[] data = md5Hash.ComputeHash (Encoding.UTF8.GetBytes (source.ToString()));
                for (int i = 0; i < data.Length; i++)
                {
                    result.Append (data[i].ToString ("x2"));
                }

            }

            return result.ToString ();
        }

        private string GenerateBaseUrl()
        {
            return string.Format ("https://{0}.przelewy24.pl", this.P24Mode.ToString());
        }

        #endregion

        public bool TestMD5()
        {
            using (MD5 md5Hash = MD5.Create ())
            {
                string input = "49518|d13be93f322c7ec3";
                Console.WriteLine("Input: '{0}'", input);

                byte[] data = md5Hash.ComputeHash (Encoding.UTF8.GetBytes (input));
                StringBuilder stb = new StringBuilder ();
                for (int i = 0; i < data.Length; i++)
                {
                    stb.Append (data [i].ToString ("x2"));
                }

                Console.WriteLine ("Result: {0}",stb.ToString ());
            }

            return false;
        }
    }
}

