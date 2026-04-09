using Unity.VisualScripting;
using UnityEngine;

public class buyGunHandler : MonoBehaviour
{
    //player money

    public void buyGun(Guns gunToBuy)
    {
        switch (gunToBuy) 
        {
            //subtract player money then give them the gun
            case Guns.pistol: break;
            case Guns.assault_rifle: break;
            case Guns.revolver: break;
            case Guns.smg: break;
            case Guns.bolt_action_rifle: break;
            case Guns.shotgun: break;
            case Guns.shotgun_sawn_off: break;
            case Guns.semi_auto_rifle: break;
        }
    
    }

}
