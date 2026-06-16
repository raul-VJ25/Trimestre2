using UnityEngine;
using System.IO;
using System;
using System.Security.Cryptography;
using System.Text;

// Gestor de guardado y carga de partidas
// Utiliza JSON encriptado con timestamps para persistencia robusta
public static class SaveManager
{
    private static string SaveFolder => Path.Combine(Application.persistentDataPath, "savedata");
    private static string BackupFolder => Path.Combine(Application.persistentDataPath, "savedata_backup");

    // Clave simple para encriptación XOR (no es seguridad real, pero demuestra el concepto)
    private const string ENCRYPTION_KEY = "RogueRaul2026Key";

    // Guarda los datos del jugador en un archivo JSON encriptado con timestamp
    public static void SavePlayerData(PlayerData playerData)
    {
        try
        {
            if (!Directory.Exists(SaveFolder))
            {
                Directory.CreateDirectory(SaveFolder);
            }

            string safeName = string.Join("_", playerData.Name.Split(Path.GetInvalidFileNameChars()));
            string filePath = Path.Combine(SaveFolder, safeName + ".player");

            // Crear backup antes de sobrescribir
            CreateBackup(filePath);

            // Serializar a JSON
            string json = JsonUtility.ToJson(playerData, true);

            // Añadir timestamp
            string dataWithTimestamp = $"{{\"timestamp\":\"{DateTime.Now:yyyy-MM-dd HH:mm:ss}\",\"data\":{json.Substring(1)}";

            // Encriptar
            string encrypted = Encrypt(dataWithTimestamp);

            // Guardar
            File.WriteAllText(filePath, encrypted);
            Debug.Log($"Partida guardada en: {filePath} ({DateTime.Now:HH:mm:ss})");
        }
        catch (Exception e)
        {
            Debug.LogError($"Error al guardar partida: {e.Message}");
        }
    }

    // Carga los datos de un jugador desde archivo encriptado
    public static PlayerData LoadPlayerData(string fileName)
    {
        try
        {
            string filePath = Path.Combine(SaveFolder, fileName);
            if (!File.Exists(filePath))
            {
                Debug.LogWarning($"Archivo no encontrado: {filePath}");
                return null;
            }

            string encrypted = File.ReadAllText(filePath);
            string decrypted = Decrypt(encrypted);

            // Extraer timestamp (opcional, para logs)
            int dataStart = decrypted.IndexOf("\"data\":") + 7;
            string json = "{" + decrypted.Substring(dataStart);

            PlayerData data = JsonUtility.FromJson<PlayerData>(json);
            Debug.Log($"Partida cargada: {fileName}");
            return data;
        }
        catch (Exception e)
        {
            Debug.LogError($"Error al cargar partida {fileName}: {e.Message}");
            return null;
        }
    }

    // Obtiene todos los archivos de guardado con información de timestamp
    public static string[] GetAllSaveFiles()
    {
        try
        {
            if (!Directory.Exists(SaveFolder))
            {
                return new string[0];
            }
            return Directory.GetFiles(SaveFolder, "*.player");
        }
        catch (Exception e)
        {
            Debug.LogError($"Error al listar archivos de guardado: {e.Message}");
            return new string[0];
        }
    }

    // Obtiene el timestamp de un archivo de guardado
    public static DateTime GetSaveTimestamp(string filePath)
    {
        try
        {
            if (!File.Exists(filePath)) return DateTime.MinValue;

            string encrypted = File.ReadAllText(filePath);
            string decrypted = Decrypt(encrypted);

            int start = decrypted.IndexOf("\"timestamp\":\"") + 13;
            int end = decrypted.IndexOf("\"", start);
            string timestampStr = decrypted.Substring(start, end - start);

            return DateTime.Parse(timestampStr);
        }
        catch
        {
            return File.GetLastWriteTime(filePath);
        }
    }

    // Crea un backup del archivo antes de sobrescribirlo
    private static void CreateBackup(string filePath)
    {
        try
        {
            if (!File.Exists(filePath)) return;

            if (!Directory.Exists(BackupFolder))
            {
                Directory.CreateDirectory(BackupFolder);
            }

            string fileName = Path.GetFileName(filePath);
            string backupPath = Path.Combine(BackupFolder, fileName + ".backup");

            // Mantener solo el último backup
            if (File.Exists(backupPath))
            {
                File.Delete(backupPath);
            }

            File.Copy(filePath, backupPath);
            Debug.Log($"Backup creado: {backupPath}");
        }
        catch (Exception e)
        {
            Debug.LogWarning($"No se pudo crear backup: {e.Message}");
        }
    }

    // Encriptación simple XOR
    private static string Encrypt(string text)
    {
        byte[] textBytes = Encoding.UTF8.GetBytes(text);
        byte[] keyBytes = Encoding.UTF8.GetBytes(ENCRYPTION_KEY);

        for (int i = 0; i < textBytes.Length; i++)
        {
            textBytes[i] = (byte)(textBytes[i] ^ keyBytes[i % keyBytes.Length]);
        }

        return Convert.ToBase64String(textBytes);
    }

    // Desencriptación XOR
    private static string Decrypt(string encrypted)
    {
        try
        {
            byte[] encryptedBytes = Convert.FromBase64String(encrypted);
            byte[] keyBytes = Encoding.UTF8.GetBytes(ENCRYPTION_KEY);

            for (int i = 0; i < encryptedBytes.Length; i++)
            {
                encryptedBytes[i] = (byte)(encryptedBytes[i] ^ keyBytes[i % keyBytes.Length]);
            }

            return Encoding.UTF8.GetString(encryptedBytes);
        }
        catch
        {
            Debug.LogError("Error al desencriptar archivo de guardado");
            return "{}";
        }
    }
}