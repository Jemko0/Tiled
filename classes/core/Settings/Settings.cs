using System;
using System.IO;

namespace Tiled.User
{
    public struct UserSettingsData
    {
        public float volumeSFX;
        public float volumeMusic;
        public float camZoom;
    }
    public class Settings
    {
        public UserSettingsData data = new UserSettingsData();

        public bool WriteSettings(string path)
        {
            try
            {
                Stream stream = File.OpenRead(path);
                BinaryWriter writer = new BinaryWriter(stream);

                writer.Write(data.volumeSFX);
                writer.Write(data.volumeMusic);
                writer.Write(data.camZoom);

                return true;
            }
            catch (Exception ex)
            {
                Main.netClient.externClientInvokeException(ex);
                return false;
            }
            
        }

        public bool LoadSettings(string path)
        {
            try
            {
                Stream stream = File.OpenRead(path);
                BinaryReader reader = new BinaryReader(stream);

                data.volumeSFX = reader.ReadSingle();
                data.volumeMusic = reader.ReadSingle();
                data.camZoom = reader.ReadSingle();

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public static Settings GetUserSettings()
        {
            return Main.userSettings;
        }

        public void ApplySettings()
        {
            var g = Program.GetGame();

            //g.localCamera.zoom = data.camZoom;
        }
    }
}
