using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using FirebaseAdmin;
using FirebaseAdmin.Auth;
using Google.Apis.Auth.OAuth2;

namespace HeyDo.Controllers
{
    public class AuthController : Controller
    {
        private static string jsonCred = "{\n  \"type\": \"service_account\",\n  \"project_id\": \"heydo-f20ff\",\n  \"private_key_id\": \"b4429f8cd32564def39b9ddac7a91e6fd0ec175b\",\n  \"private_key\": \"-----BEGIN PRIVATE KEY-----\\nMIIEuwIBADANBgkqhkiG9w0BAQEFAASCBKUwggShAgEAAoIBAQDriEzj0LMcMaCw\\nzOBzytvUxLgBeLDFDOJCeO6dIHjaqA/0LTA6fI6JJ+Jh1olgb73unqB7cAbYBkLS\\nlF2D5k4VjlnhCYVB5SbRxseOA7izRK2h4vBX/FCqw3gvAFa6tfG1xz31AWvvaKKl\\nwrn4jVwtpi72HP7ysiDppPbGIhl6uYgx8qqmxqrLgE2GqNf2/rcav2b0NBI6sDSc\\nWR287ZS0h9cvBtWGhEsTkiZhgfalAYEOPv/5MKQXfvmQhXglNuQ6SWl/ZB8EK0Ne\\n57R1G0WxzUNf+VEPIgDS9RQ2FB8G+X16BKU4e2dfbLHLawACGpHmacLRtj7M8QJy\\nMG2CqaDhAgMBAAECgf9kH2zDUaE4bi0alMnu1ZOqWHWTKo4tnDlFrvxZwi2dxa01\\nsn/t9ngg/EA5Y8zCOFex0a/zlfIforzroP/S/vPyj32YzBNGdildkFw3YoQBfaoF\\nFBOKhrFD5hQQ5tpd3gcCEjbXdkmlXAJhtIVmkBdY0GwZWZO6V6TmE9ogH7hbeL8S\\nJfnkyqZhABxEGCDRqiBx8br1NBwknqUtX2UKeo8CzEUx8n+lt9C37R64TtpGHPz4\\nNZTKFzuOXigWZWmXeWN3TpnbzpdrQEZ40vlhXX81hHkwyRNnA2tShH/mzwrF966I\\n8WIhnwUFA3W6xGZ3P3iJUAu26geh6deLLPzCMAECgYEA+Aqb5Kff8ScBW1n2P3ua\\ngObBr8Ml/m3fohMj7Spj/CqiSPlyNTCekhRt1eD4gAIJ/oOB+Koti7gJ+WmQZk+1\\n1EEa62lEQqgPyWp9U78mVJrJlugBpb6CrsM4rjhhthifPJj/qQgOOoRgM6pAOse+\\n9/L/YViYY844aNw24a+opAECgYEA8xbxgnERjcfmEKiKYhxA/prb9SwnJb6/i4t1\\nmAvOstnc5VLEkjvdOWkjKlfkY678TdRu7VFQFZ28XbTcBEcJ9bVTRHl+mn2HaAW1\\nBE9Mrlec2zQjgMPbUJDp7K39WUVear74Hx/FzLkYp+alb9EmpRpT5gaXE4TxAAQk\\no8wBfOECgYEArajf1IskYvZoIMkveurTFYR5/tCmfSf39sVn1dhkAFzpGiZrK4pK\\nuwPKkn/b++Nxk9wG105ubPABK7oY+0i7iUu7yS3+OJzp0J7Z+BRQhdOJaDovTRfA\\n7xKHUJLw9kju08Ja3MFjFN0xbXq5VW4b6gUbf9BDgT0UiXYEhqYxcAECgYBgvGCf\\naB90LHiBaJCpOlUi/L1Lc6agfqf7ACRrvUckJU6ic62GBAylLBGyaAebI6eD0WXT\\nSuhzsbGkGLT1fs+X1/Cq7y5SjGG2I49Fh8kcGAFzuevm8gn0pbZxWUvYmqJYGGbk\\n+9/a+E23gzGSrciTK0b34ud7/Qtq1js+c5CkAQKBgBfg9LvYBMyJnVM4Ga0xUoGf\\nSdlRH0ysYxQkRFBfIlm/lWM51OdwUt/Yem39+RvTOOetBhJp4/Syo1S/w3qCMD9j\\nsJgqJOcSoeOWU/yR0hlXXrz2Tldom6E/7ii3RcXKBierqmCQEjf6nbPavy/5mv9d\\nQeG0yJZuCcdw7+0jnKo4\\n-----END PRIVATE KEY-----\\n\",\n  \"client_email\": \"firebase-adminsdk-3ajeu@heydo-f20ff.iam.gserviceaccount.com\",\n  \"client_id\": \"107576547082853530243\",\n  \"auth_uri\": \"https://accounts.google.com/o/oauth2/auth\",\n  \"token_uri\": \"https://oauth2.googleapis.com/token\",\n  \"auth_provider_x509_cert_url\": \"https://www.googleapis.com/oauth2/v1/certs\",\n  \"client_x509_cert_url\": \"https://www.googleapis.com/robot/v1/metadata/x509/firebase-adminsdk-3ajeu%40heydo-f20ff.iam.gserviceaccount.com\"\n}";
        public static async Task<string> Google(string idToken)
        {
            //TODO can theoretically check id token here and return uid
            //need to figure out how to log out of this though if even necessary
            try
            {
                var defaultApp = FirebaseApp.DefaultInstance;  

                if (defaultApp == null)
                {
                    FirebaseApp.Create(new AppOptions()
                    {
                        Credential = GoogleCredential.FromJson(jsonCred),
                    });
                }

                var decoded = await FirebaseAuth.DefaultInstance.VerifyIdTokenAsync(idToken);
                var uid = decoded.Uid;

                return uid;
            }
            catch (Exception e)
            {
                return e.Message;
            }       
        }

        public static void Clear()
        {
            //Unsure if this would ever need to be used
            try
            {
                FirebaseApp.DefaultInstance.Delete();
            }
            catch (Exception e)
            {
                throw;
            }
        }       
    }
}