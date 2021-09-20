using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TowerPlacement : MonoBehaviour
{
    private Tower _placedTower;

    // Fungsi yang terpanggil sekali ketika ada object Rigidbody yang menyentuh area collider
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (_placedTower != null)
        {
            return;
        }
        Tower tower = collision.GetComponent<Tower>();
        //Debug.Log($"Entered : {tower?.name}; Placed : {_placedTower?.name}");

        if (tower != null)
        {
            tower.SetPlacePosition(transform.position);
            _placedTower = tower;
            //Debug.Log($"Success Enter : {tower?.name}; Placed : {_placedTower?.name}");
        }
    }

    // Kebalikan dari OnTriggerEnter2D, fungsi ini terpanggil sekali ketika object tersebut meninggalkan area collider
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (_placedTower == null)
        {
            return;
        }
        Tower tower = collision.GetComponent<Tower>();
        //Debug.Log($"Exit : {tower?.name}; Placed : {_placedTower?.name}");

        if (tower != null && tower == _placedTower)
        {
            _placedTower.ResetPlacePosition();
            _placedTower = null;
            //Debug.Log($"Success Exit : {tower?.name}; Placed : {_placedTower?.name}");
        }
    }
}
