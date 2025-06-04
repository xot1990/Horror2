using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightSwitch : MonoBehaviour
{
    
		public Animator SwitchAnimator;
		public bool open;
		public Transform Player;
		public List<Light> LightPool;
		public bool isUse;

		void Start()
		{
			open = false;
		}

		void OnMouseOver()
		{
			{
				if (Player)
				{
					float dist = Vector3.Distance(Player.position, transform.position);
					if (dist < 15)
					{
						if (open == false)
						{
							if (Input.GetMouseButtonDown(0))
							{
								StartCoroutine(On());
							}
						}
						else
						{
							if (open == true)
							{
								if (Input.GetMouseButtonDown(0))
								{
									StartCoroutine(Off());
								}
							}

						}

					}
				}

			}

		}

		IEnumerator On()
		{
			SwitchAnimator.Play("On");
			open = true;
			yield return new WaitForSeconds(.5f);
			if (!isUse)
			{
				isUse = true;
				QuestEventBus.GetDoneAction("LightOn");
			}
			foreach (var L in LightPool)
			{
				L.intensity = 1;
			}
		}

		IEnumerator Off()
		{
			SwitchAnimator.Play("Off");
			open = false;
			yield return new WaitForSeconds(.5f);
			foreach (var L in LightPool)
			{
				L.intensity = 0;
			}
		}


}
