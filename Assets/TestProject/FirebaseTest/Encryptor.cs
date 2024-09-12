using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;
using Cysharp.Threading.Tasks;

public class Encryptor : IDisposable
{    
    private static Encryptor _defalut;    
    public static Encryptor Default => _defalut ??= new Encryptor(Encoding.UTF8.GetBytes("ITzKFvm5VRwj2oyJAytAHbzEqru9oAHY"), new byte[] { 15, 57, 69, 79, 204, 145, 125, 100, 95, 134, 188, 50, 49, 171, 251, 234 });

    private RijndaelManaged _mgr;
    public ICryptoTransform CreateEncryptor() => _mgr.CreateEncryptor();
    public ICryptoTransform CreateDecryptor() => _mgr.CreateDecryptor();


    public Encryptor(string key, string iv)
    {
        Set(Encoding.Default.GetBytes(key), Encoding.Default.GetBytes(iv));        
    }

    public Encryptor(byte[] key, byte[] iv)
    {
        Set(key, iv);
    }    

    public void Dispose()
    {
        if (_mgr != null)
        {
            _mgr.Clear();
            _mgr.Dispose();
            _mgr = null;
        }
    }

    public void Set(byte[] key, byte[] iv)
    {
        _mgr?.Clear();
        _mgr?.Dispose();

        _mgr = new RijndaelManaged();
        _mgr.Mode = CipherMode.CBC;
        _mgr.Padding = PaddingMode.PKCS7;
        _mgr.KeySize = 256;
        _mgr.BlockSize = 128;
        _mgr.Key = key;
        _mgr.IV = iv;
    }

    public byte[] Encrypt(byte[] data)
    {
        byte[] result;
        using (MemoryStream ms = new MemoryStream())
        {
            ms.Write(data, this);            
            result = ms.ToArray();
        }

        return result;
    }

    public byte[] Encrypt(string data)
    {
        byte[] result;
        using (MemoryStream ms = new MemoryStream())
        {
            ms.Write(data, this);
            result = ms.ToArray();
        }

        return result;
    }    

    public async UniTask<byte[]> EncryptAsync(string data)
    {
        byte[] result;
        using (MemoryStream ms = new MemoryStream())
        {
            await ms.WriteAsync(data, this);
            result = ms.ToArray();
        }

        return result;
    }

    public string EncryptToString(string data)
    {        
        return Convert.ToBase64String(Encrypt(data));
    }

    public async UniTask<string> EncryptToStringAsync(string data)
    {
        return Convert.ToBase64String(await EncryptAsync(data));
    }

    public byte[] Decrypt(byte[] data)
    {             
        byte[] result;        
        using (MemoryStream ms = new MemoryStream(data))        
            result = ms.ReadBytes(this);

        return result;
    }

    public async UniTask<byte[]> DecryptAsync(byte[] data)
    {
        byte[] result;
        using (MemoryStream ms = new MemoryStream(data))
            result = await ms.ReadBytesAsync(this);

        return result;
    }

    public string DecryptToString(byte[] data)
    {
        string result;
        using (MemoryStream ms = new MemoryStream(data))        
            result = ms.ReadString(this);

        return result;
    }

    public async UniTask<string> DecryptToStringAsync(byte[] data)
    {
        string result;
        using (MemoryStream ms = new MemoryStream(data))
            result = await ms.ReadStringAsync(this);

        return result;
    }

    public string DecryptToString(string data)
    {
        return DecryptToString(Convert.FromBase64String(data));
    }

    public async UniTask<string> DecryptToStringAsync(string data)
    {
        return await DecryptToStringAsync(Convert.FromBase64String(data));
    }
}

public static class EncryptorExtension
{    
    public static void Write(this Stream stream, byte[] data, in Encryptor crypto)
    {
        using (ICryptoTransform encryptor = crypto.CreateEncryptor())
        {
            using (CryptoStream cs = new CryptoStream(stream, encryptor, CryptoStreamMode.Write))
            {
                using (BinaryWriter bw = new BinaryWriter(cs))
                {
                    bw.Write(data);
                }
            }
        }
    }

    public static async UniTask WriteAsync(this Stream stream, byte[] data, Encryptor crypto)
    {
        using (ICryptoTransform encryptor = crypto.CreateEncryptor())
        {
            using (CryptoStream cs = new CryptoStream(stream, encryptor, CryptoStreamMode.Write))
            {
                await cs.WriteAsync(data, 0, data.Length).AsUniTask();                
            }
        }
    }

    public static void Write(this Stream stream, string data, Encryptor crypto)
    {
        using (ICryptoTransform encryptor = crypto.CreateEncryptor())
        {
            using (CryptoStream cs = new CryptoStream(stream, encryptor, CryptoStreamMode.Write))
            {
                using (StreamWriter sw = new StreamWriter(cs))
                {
                    sw.Write(data);
                }
            }
        }
    }

    public static async UniTask WriteAsync(this Stream stream, string data, Encryptor crypto)
    {
        using (ICryptoTransform encryptor = crypto.CreateEncryptor())
        {
            using (CryptoStream cs = new CryptoStream(stream, encryptor, CryptoStreamMode.Write))
            {
                using (StreamWriter sw = new StreamWriter(cs))
                {
                    await sw.WriteAsync(data).AsUniTask();
                }
            }
        }
    }

    public static string ReadString(this Stream stream, Encryptor crypto)
    {
        string result;
        using (ICryptoTransform decryptor = crypto.CreateDecryptor())
        {
            using (CryptoStream cs = new CryptoStream(stream, decryptor, CryptoStreamMode.Read))
            {
                using (StreamReader sr = new StreamReader(cs))
                {
                    result = sr.ReadToEnd();
                }
            }
        }
        return result;
    }

    public static async UniTask<string> ReadStringAsync(this Stream stream, Encryptor crypto)
    {
        string result;
        using (ICryptoTransform decryptor = crypto.CreateDecryptor())
        {
            using (CryptoStream cs = new CryptoStream(stream, decryptor, CryptoStreamMode.Read))
            {
                using (StreamReader sr = new StreamReader(cs))
                {
                    result = await sr.ReadToEndAsync().AsUniTask();
                }
            }
        }
        return result;
    }

    public static byte[] ReadBytes(this Stream stream, Encryptor crypto)
    {        
        if (stream.Length > int.MaxValue)
        {
            return ReadLongBytes(stream, crypto);
        }

        byte[] result;
        using (ICryptoTransform decryptor = crypto.CreateDecryptor())
        {
            using (CryptoStream cs = new CryptoStream(stream, decryptor, CryptoStreamMode.Read))
            {                
                using (BinaryReader br = new BinaryReader(cs))
                {
                    result = br.ReadBytes((int)stream.Length);                                        
                }
            }
        }

        return result;
    }

    public static async UniTask<byte[]> ReadBytesAsync(this Stream stream, Encryptor crypto)
    {
        byte[] result;
        using (ICryptoTransform decryptor = crypto.CreateDecryptor())
        {
            using (CryptoStream cs = new CryptoStream(stream, decryptor, CryptoStreamMode.Read))
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    await cs.CopyToAsync(ms).AsUniTask();
                    result = ms.ToArray();
                }
            }
        }
        return result;
    }

    private static byte[] ReadLongBytes(this Stream stream, Encryptor crypto)
    {
        byte[] result;        
        using (ICryptoTransform decryptor = crypto.CreateDecryptor())
        {
            using (CryptoStream cs = new CryptoStream(stream, decryptor, CryptoStreamMode.Read))
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    cs.CopyTo(ms);
                    result = ms.ToArray();
                }                
            }
        }        
        return result;
    }

    public static async UniTask WriteAllBytesAsyncToFile(this Encryptor encryptor, string path, byte[] bytes)
    {        
        using (var fileStream = new FileStream(path, FileMode.Create))        
            await fileStream.WriteAsync(bytes, encryptor);        
    }

    public static byte[] ReadAllBytesFromFile(this Encryptor encryptor, string path)
    {
        byte[] result = null;
        using (var fileStream = new FileStream(path, FileMode.Open))
            result = fileStream.ReadBytes(encryptor);

        return result;
    }

    public static async UniTask<byte[]> ReadAllBytesAsyncFromFile(this Encryptor encryptor, string path)
    {
        byte[] result = null;
        using (var fileStream = new FileStream(path, FileMode.Open))
            result = await fileStream.ReadBytesAsync(encryptor);

        return result;
    }

    public static string ReadAllTextFromFile(this Encryptor encryptor, string path)
    {
        string result;
        using (var fileStream = new FileStream(path, FileMode.Open))
            result = fileStream.ReadString(encryptor);

        return result;
    }

    public static async UniTask<string> ReadAllTextAsyncFromFile(this Encryptor encryptor, string path)
    {
        string result;
        using (var fileStream = new FileStream(path, FileMode.Open))
            result = await fileStream.ReadStringAsync(encryptor);

        return result;
    }
}