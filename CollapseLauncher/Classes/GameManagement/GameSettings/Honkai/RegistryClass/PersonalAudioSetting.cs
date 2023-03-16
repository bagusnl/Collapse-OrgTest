﻿using CollapseLauncher.GameSettings.Honkai.Context;
using CollapseLauncher.Interfaces;
using Hi3Helper;
using Microsoft.Win32;
using System;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using static CollapseLauncher.GameSettings.Statics;
using static Hi3Helper.Data.ConverterTool;
using static Hi3Helper.Logger;

namespace CollapseLauncher.GameSettings.Honkai
{
    internal class PersonalAudioSetting : IGameSettingsValue<PersonalAudioSetting>
    {
        #region Fields
        private const string _ValueName = "GENERAL_DATA_V2_PersonalAudioSetting_h3869048096";
        private PersonalAudioSettingVolume _VolumeValue;
        private int _MasterVolume = 100;
        private int _BGMVolume = 100;
        private int _SoundEffectVolume = 100;
        private int _VoiceVolume = 100;
        private int _ElfVolume = 100;
        private int _CGVolumeV2 = 100;

        public PersonalAudioSetting()
        {
            _VolumeValue = PersonalAudioSettingVolume.Load();
        }
        #endregion

        #region Properties
        /// <summary>
        /// This defines "<c>Master Volume</c>" slider In-game settings -> Audio.<br/><br/>
        /// Range: 0 - 100<br/>
        /// Default: 100
        /// </summary>
        public int MasterVolume
        {
            get => _MasterVolume;
            set
            {
                _MasterVolume = value;
                _VolumeValue.MasterVolumeValue = value;
            }
        }

        /// <summary>
        /// This defines "<c>BGM</c>" slider In-game settings -> Audio -> Volume Balance.<br/><br/>
        /// Range: 0 - 100<br/>
        /// Default: 100
        /// </summary>
        public int BGMVolume
        {
            get => _BGMVolume;
            set
            {
                _BGMVolume = value;
                _VolumeValue.BGMVolumeValue = ConvertRangeValue(0.0f, 100.0f, value, 0.0f, 3.0f);
            }
        }

        /// <summary>
        /// This defines "<c>SFX</c>" slider In-game settings -> Audio -> Volume Balance.<br/><br/>
        /// Range: 0 - 100<br/>
        /// Default: 100
        /// </summary>
        public int SoundEffectVolume
        {
            get => _SoundEffectVolume;
            set
            {
                _SoundEffectVolume = value;
                _VolumeValue.SoundEffectVolumeValue = ConvertRangeValue(0.0f, 100.0f, value, 0.0f, 3.0f);
            }
        }

        /// <summary>
        /// This defines "<c>Voice Acting</c>" slider In-game settings -> Audio -> Volume Balance.<br/><br/>
        /// Range: 0 - 100<br/>
        /// Default: 100
        /// </summary>
        public int VoiceVolume
        {
            get => _VoiceVolume;
            set
            {
                _VoiceVolume = value;
                _VolumeValue.VoiceVolumeValue = ConvertRangeValue(0.0f, 100.0f, value, 0.0f, 3.0f);
            }
        }

        /// <summary>
        /// This defines "<c>ELF VO</c>" slider In-game settings -> Audio -> Volume Balance.<br/><br/>
        /// Range: 0 - 100<br/>
        /// Default: 100
        /// </summary>
        public int ElfVolume
        {
            get => _ElfVolume;
            set
            {
                _ElfVolume = value;
                _VolumeValue.ElfVolumeValue = ConvertRangeValue(0.0f, 100.0f, value, 0.0f, 3.0f);
            }
        }

        /// <summary>
        /// This defines "<c>CG</c>" slider In-game settings -> Audio -> Volume Balance.<br/><br/>
        /// Range: 0 - 100<br/>
        /// Default: 100
        /// </summary>
        public int CGVolumeV2
        {
            get => _CGVolumeV2;
            set
            {
                _CGVolumeV2 = value;
                _VolumeValue.CGVolumeValue = ConvertRangeValue(0.0f, 100.0f, value, 0.0f, 1.8f);
            }
        }

        /// <summary>
        /// This defines "<c>Mute</c>" switch In-game settings -> Audio.<br/><br/>
        /// Range: 0 - 100<br/>
        /// Default: 100
        /// </summary>
        public bool Mute { get; set; } = false;

        /// <summary>
        /// This stays as "<c>Japanese</c>" whenever <c>_userCVLanguage</c> has set to any value.<br/><br/>
        /// Default: "Japanese"
        /// </summary>
        public string CVLanguage { get; set; } = "Japanese";

        /// <summary>
        /// This defines "<c>Voice-over</c>" radiobox In-game settings -> Audio.<br/><br/>
        /// Values:<br/>
        ///     - 1 = Japanese<br/>
        ///     - 0 = Chinese(PRC)<br/><br/>
        /// Default: 1 (Japanese)
        /// </summary>
        [JsonIgnore]
        public int _userCVLanguageInt
        {
            get => _userCVLanguage switch
            {
                "Japanese" => 1,
                _ => 0,
            };
            set => _userCVLanguage = value switch
            {
                1 => "Japanese",
                _ => "Chinese(PRC)",
            };
        }

        /// <summary>
        /// This defines "<c>Voice-over</c>" radiobox In-game settings -> Audio.<br/><br/>
        /// Values:<br/>
        ///     - Japanese<br/>
        ///     - Chinese(PRC)<br/><br/>
        /// Default: "Japanese"
        /// </summary>
        public string _userCVLanguage { get; set; } = "Japanese";

        /// <summary>
        /// Default: true
        /// </summary>
        public bool IsUserDefined { get; set; } = true;
        #endregion

        #region Methods
#nullable enable
        public static PersonalAudioSetting Load()
        {
            try
            {
                if (RegistryRoot == null) throw new NullReferenceException($"Cannot load {_ValueName} RegistryKey is unexpectedly not initialized!");

                object? value = RegistryRoot.GetValue(_ValueName, null);

                if (value != null)
                {
                    ReadOnlySpan<byte> byteStr = (byte[])value;
                    return (PersonalAudioSetting?)JsonSerializer.Deserialize(byteStr.Slice(0, byteStr.Length - 1), typeof(PersonalAudioSetting), PersonalAudioSettingContext.Default) ?? new PersonalAudioSetting();
                }
            }
            catch (Exception ex)
            {
                LogWriteLine($"Failed while reading {_ValueName}\r\n{ex}", LogType.Error, true);
            }

            return new PersonalAudioSetting();
        }

        public void Save()
        {
            try
            {
                if (RegistryRoot == null) throw new NullReferenceException($"Cannot save {_ValueName} since RegistryKey is unexpectedly not initialized!");

                string data = JsonSerializer.Serialize(this, typeof(PersonalAudioSetting), PersonalAudioSettingContext.Default) + '\0';
                byte[] dataByte = Encoding.UTF8.GetBytes(data);

                RegistryRoot.SetValue(_ValueName, dataByte, RegistryValueKind.Binary);
                _VolumeValue.Save();
            }
            catch (Exception ex)
            {
                LogWriteLine($"Failed to save {_ValueName}!\r\n{ex}", LogType.Error, true);
            }
        }

        public bool Equals(PersonalAudioSetting? comparedTo)
        {
            if (ReferenceEquals(this, comparedTo)) return true;
            if (comparedTo == null) return false;

            return comparedTo.IsUserDefined == this.IsUserDefined &&
                comparedTo.CVLanguage == this.CVLanguage &&
                comparedTo._userCVLanguage == this._userCVLanguage &&
                comparedTo.BGMVolume == this.BGMVolume &&
                comparedTo.ElfVolume == this.ElfVolume &&
                comparedTo.CGVolumeV2 == this.CGVolumeV2 &&
                comparedTo.MasterVolume == this.MasterVolume &&
                comparedTo.SoundEffectVolume == this.SoundEffectVolume &&
                comparedTo.VoiceVolume == this.VoiceVolume &&
                comparedTo.Mute == this.Mute;
        }
#nullable disable
        #endregion
    }
}
