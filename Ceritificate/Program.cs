using System;
using System.IO;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

class Program
{
    static void Main(string[] args)
    {
        // Generate private and public certificates and save them in files
        using (RSA rsa = RSA.Create())
        {
            RSAParameters publicKey = rsa.ExportParameters(false);
            RSAParameters privateKey = rsa.ExportParameters(true);

            string publicKeyXml = GetKeyString(publicKey);
            string privateKeyXml = GetKeyString(privateKey);

            File.WriteAllText("publickey.crt", publicKeyXml);
            File.WriteAllText("privatekey.key", privateKeyXml);
        }

        // Generate a self-signed certificate in pfx format
        X509Certificate2 certificate = new X509Certificate2();
        using (RSA rsa = RSA.Create())
        {
            var certificateRequest = new CertificateRequest("cn=localhost", rsa, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
            certificate = certificateRequest.CreateSelfSigned(DateTimeOffset.Now.AddDays(-1), DateTimeOffset.Now.AddDays(365)); // you can update days of expiration accordingly
        }
        byte[] pfxBytes = certificate.Export(X509ContentType.Pfx, "password"); // update the password accordingly
        File.WriteAllBytes("certificate.pfx", pfxBytes); // update the location of your certificate accordingly

        // Store the certificate in the local certificate store (Personal)
        // for this you haveto follow following steps
        /*
         * Open the Microsoft Management Console (MMC) by typing mmc in the Start menu or in the Run dialog box.
            Click on File > Add/Remove Snap-in....
            Select the Certificates snap-in and click Add.
            Select Computer account and click Next.
            Select Local computer and click Finish.
            Click OK.
         */
        // if it still gives error just run you VS with admin permissions
        X509Store store = new X509Store(StoreName.My, StoreLocation.LocalMachine);
        store.Open(OpenFlags.ReadWrite);
        store.Add(certificate);
        store.Close();
    }

    static string GetKeyString(RSAParameters parameters)
    {
        using (RSA rsa = RSA.Create())
        {
            rsa.ImportParameters(parameters);
            return rsa.ToXmlString(false);
        }
    }
}
