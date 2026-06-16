using UnityEngine;
using System.IO;
using System;

// Gestor de guardado y carga de partidas
// Utiliza JSON para serializar datos de jugadores
public static class SaveManager
{
    private static string SaveFolder => Path.Combine(Application.persistentDataPath, "savedata");

    // Guarda los datos del jugador en un archivo JSON
    public static void SavePlayerData(PlayerData playerData)
    {
        if (!Directory.Exists(SaveFolder))
        {
            Directory.CreateDirectory(SaveFolder);
        }

        string safeName = string.Join("_", playerData.Name.Split(Path.GetInvalidFileNameChars()));
        string filePath = Path.Combine(SaveFolder, safeName + ".player");

        string json = JsonUtility.ToJson(playerData, true);
        File.WriteAllText(filePath, json);

        Debug.Log($"Partida guardada en: {filePath}");
    }

    // Carga los datos de un jugador desde archivo
    public static PlayerData LoadPlayerData(string fileName)
    {
        string filePath = Path.Combine(SaveFolder, fileName);

        if (File.Exists(filePath))
        {
            string contenidoJSON = File.ReadAllText(filePath);
            PlayerData data = JsonUtility.FromJson<PlayerData>(contenidoJSON);
            return data;
        }

        Debug.LogWarning($"Archivo no encontrado: {filePath}");
        return null;
    }

    // Obtiene todos los archivos de guardado
    public static string[] GetAllSaveFiles()
    {
        if (!Directory.Exists(SaveFolder))
        {
            return new string[0];
        }

        return Directory.GetFiles(SaveFolder, "*.player");
    }
}