using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This class manages anything related to the updating of MK values.
/// </summary>
public class WeaponMKManager : MonoBehaviour {

    [SerializeField]
    public WeaponMaterialManager steamHammerWeaponSkin;
    [SerializeField]
    public WeaponMaterialManager steamCannonWeaponSkin;
    [SerializeField]
    public WeaponMaterialManager steamMagnetWeaponSkin;
    [SerializeField]
    public WeaponMaterialManager steamRailGunWeaponSkin;
    [SerializeField]
    public WeaponMaterialManager teslaRayWeaponSkin;
    [SerializeField]
    public WeaponMaterialManager steamSawWeaponSkin;

    [Header("MK Pages references")]
    [SerializeField]
    private TextureChanger steamHammerMKPage;
    [SerializeField]
    private TextureChanger steamCannonMKPage;
    [SerializeField]
    private TextureChanger steamMagnetMKPage;
    [SerializeField]
    private TextureChanger steamRailGunMKPage;
    [SerializeField]
    private TextureChanger teslaRayMKPage;
    [SerializeField]
    private TextureChanger steamSawMKPage;

    private int steamHammerMK; 


    private GameControlls gameControlls;

    // Use this for initialization
    void Start () {
        gameControlls = GameObject.Find("[DONT_DESTROY_ON_LOAD]").GetComponent<GameControlls>();
        GetWeaponsCurrentValuesFromGC();
        CalculateUpgradedWeapons(); 
    }

    /// <summary>
    /// Gets the weapons dictionary from the GC and goes through it. Sets the material of the weapon and the texture of the mk page based on the VALUE value in the dictionary
    /// </summary>
    void GetWeaponsCurrentValuesFromGC()
    {
        Dictionary<string, int> weaponsAndValues = gameControlls.ReturnWeaponMKValuesDictionary();

        foreach (KeyValuePair<string, int> value in weaponsAndValues)
        {
            switch (value.Key)
            {
                case "STEAM_HAMMER":
                    steamHammerWeaponSkin.SetMaterialByIndex(value.Value);
                    steamHammerMKPage.SetTextureByIndex(value.Value);
                    break;

                case "STEAM_CANNON":
                    steamCannonWeaponSkin.SetMaterialByIndex(value.Value);
                    steamCannonMKPage.SetTextureByIndex(value.Value);
                    break;

                case "STEAM_MAGNET":
                    steamMagnetWeaponSkin.SetMaterialByIndex(value.Value);
                    steamMagnetMKPage.SetTextureByIndex(value.Value);
                    break;

                case "STEAM_RAILGUN":
                    steamRailGunWeaponSkin.SetMaterialByIndex(value.Value);
                    steamRailGunMKPage.SetTextureByIndex(value.Value);
                    break;

                case "TESLA_RAY":
                    teslaRayWeaponSkin.SetMaterialByIndex(value.Value);
                    teslaRayMKPage.SetTextureByIndex(value.Value);
                    break;

                case "STEAM_SAW":
                    steamSawWeaponSkin.SetMaterialByIndex(value.Value);
                    steamSawMKPage.SetTextureByIndex(value.Value);
                    break;
            }
        }

    }

    /// <summary>
    /// Gets the list of weapons that need to be upgraded from teh GC, and applies the upgrades.
    /// </summary>
    void CalculateUpgradedWeapons()
    {
        List<EndOfLevelUnlockManager.WeaponUpgrade> weaponsToBeUpgraded = gameControlls.ReturnWeaponsToUpgradeList();

        if (weaponsToBeUpgraded.Count == 0)
        {
            return;
        }

        for (int i = 0; i < weaponsToBeUpgraded.Count; i++)
        {
            UnlockWeaponSkin(weaponsToBeUpgraded[i]);
        }

        weaponsToBeUpgraded.Clear();
    }

    /// <summary>
    /// Any weapon that has to be upgraded is passed to this method. It checks to see which weapon has been passed in and then applies the neccessary upgrade.
    /// </summary>
    /// <param name="weapon"></param>
    public void UnlockWeaponSkin(EndOfLevelUnlockManager.WeaponUpgrade weapon)
    {
        string weaponType = weapon.weaponToUpgrade.ToString();
        

        if ((int)weapon.mkValue > gameControlls.ReturnAWeaponsMKValue(weaponType)) //Only upgrade a weapon if the MK value you are upgrading to is HIGHER than the current mk value. 
        {
            switch (weaponType)
            {
                case "STEAM_HAMMER":
                    steamHammerWeaponSkin.SetMaterialByIndex((int)weapon.mkValue);
                    steamHammerMKPage.SetTextureByIndex((int)weapon.mkValue);
                    
                    if ((int) weapon.mkValue==1)
                    {
                        GameControlls.Instance.UnlockSteamAchievement(SteamAchievementsNames.HARD_AS_NAILS);
                    }
                    else if ((int)weapon.mkValue == 2)
                    {
                        GameControlls.Instance.UnlockSteamAchievement(SteamAchievementsNames.SEEING_STARS);
                    }
                    break;

                case "STEAM_CANNON":
                    steamCannonWeaponSkin.SetMaterialByIndex((int)weapon.mkValue);
                    steamCannonMKPage.SetTextureByIndex((int)weapon.mkValue);
                    if ((int)weapon.mkValue == 1)
                    {
                        GameControlls.Instance.UnlockSteamAchievement(SteamAchievementsNames.HOT_AIR);
                    }
                    else if ((int)weapon.mkValue == 2)
                    {
                        GameControlls.Instance.UnlockSteamAchievement(SteamAchievementsNames.BLOW_ME);
                    }
                    break;

                case "STEAM_MAGNET":
                    steamMagnetWeaponSkin.SetMaterialByIndex((int)weapon.mkValue);
                    steamMagnetMKPage.SetTextureByIndex((int)weapon.mkValue);
                    if ((int)weapon.mkValue == 1)
                    {
                        GameControlls.Instance.UnlockSteamAchievement(SteamAchievementsNames.PUSH_ME);
                    }
                    else if ((int)weapon.mkValue == 2)
                    {
                        GameControlls.Instance.UnlockSteamAchievement(SteamAchievementsNames.FATAL_ATTRACTION);
                    }
                    break;

                case "STEAM_RAILGUN":
                    steamRailGunWeaponSkin.SetMaterialByIndex((int)weapon.mkValue);
                    steamRailGunMKPage.SetTextureByIndex((int)weapon.mkValue);
                    if ((int)weapon.mkValue == 1)
                    {
                        GameControlls.Instance.UnlockSteamAchievement(SteamAchievementsNames.MAVERICK);
                    }
                    else if ((int)weapon.mkValue == 2)
                    {
                        GameControlls.Instance.UnlockSteamAchievement(SteamAchievementsNames.JACKAL);
                    }
                    break;

                case "TESLA_RAY":
                    teslaRayWeaponSkin.SetMaterialByIndex((int)weapon.mkValue);
                    teslaRayMKPage.SetTextureByIndex((int)weapon.mkValue);
                    if ((int)weapon.mkValue == 1)
                    {
                        GameControlls.Instance.UnlockSteamAchievement(SteamAchievementsNames.GIVES_FOCUS);
                    }
                    else if ((int)weapon.mkValue == 2)
                    {
                        GameControlls.Instance.UnlockSteamAchievement(SteamAchievementsNames.MAKES_STRONGER);
                    }
                    break;

                case "STEAM_SAW":
                    steamSawWeaponSkin.SetMaterialByIndex((int)weapon.mkValue);
                    steamSawMKPage.SetTextureByIndex((int)weapon.mkValue);
                    if ((int)weapon.mkValue == 1)
                    {
                        GameControlls.Instance.UnlockSteamAchievement(SteamAchievementsNames.LOVE_SAW);
                    }
                    else if ((int)weapon.mkValue == 2)
                    {
                        GameControlls.Instance.UnlockSteamAchievement(SteamAchievementsNames.PART_OF_ME);
                    }
                    break;
            }

            gameControlls.UpgradeWeaponValue(weaponType, (int)weapon.mkValue);
        }
    }
}
