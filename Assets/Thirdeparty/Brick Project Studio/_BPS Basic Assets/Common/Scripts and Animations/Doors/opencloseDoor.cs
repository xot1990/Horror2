using System.Collections;
using UnityEngine;

namespace SojaExiles
{
    public class OpenCloseDoor : MonoBehaviour
    {
        public Animator openandclose;
        public bool open;
        public bool isCanUse;

        void Start()
        {
            open = false;
        }

        public void Use()
        {
            if (open)
            {
                StartCoroutine(closing());
            }
            else
            {
                StartCoroutine(opening());
            }
        }

        IEnumerator opening()
        {
            Debug.Log("You are opening the door");
            if (openandclose != null) openandclose.Play("Opening");
            open = true;
            yield return new WaitForSeconds(0.5f);
        }

        IEnumerator closing()
        {
            Debug.Log("You are closing the door");
            if (openandclose != null) openandclose.Play("Closing");
            open = false;
            yield return new WaitForSeconds(0.5f);
        }
    }
}