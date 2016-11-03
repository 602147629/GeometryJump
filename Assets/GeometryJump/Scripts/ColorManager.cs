/***********************************************************************************************************
 * Produced by App Advisory - http://app-advisory.com													   *
 * Facebook: https://facebook.com/appadvisory															   *
 * Contact us: https://appadvisory.zendesk.com/hc/en-us/requests/new									   *
 * App Advisory Unity Asset Store catalog: http://u3d.as/9cs											   *
 * Developed by Gilbert Anthony Barouch - https://www.linkedin.com/in/ganbarouch                           *
 ***********************************************************************************************************/




using UnityEngine;
using System.Collections;
using DG.Tweening;

/// <summary>
/// Class in charge to manage the background colors
/// </summary>
namespace AppAdvisory.GeometryJump
{
	public class ColorManager : MonoBehaviorHelper 
	{
		public Color[] colors;

		public float timeChangeColor = 10;

		bool isGameOver = false;

        //唤醒
		void Awake()
		{
			Color c = PlayerPrefsX.GetColor("BACKGROUND_COLOR", colors[0]);
			cam.backgroundColor = c;
		}

		void OnEnable()
		{
			GameManager.OnGameStart += OnGameStart;

			GameManager.OnGameOverStarted += OnGameOverStarted;

			Color c = PlayerPrefsX.GetColor("BACKGROUND_COLOR", colors[0]);
			cam.backgroundColor = c;
		}

		void OnDisable()
		{
			GameManager.OnGameStart -= OnGameStart;

			GameManager.OnGameOverStarted -= OnGameOverStarted;

		}

		void OnGameStart()
		{
			isGameOver = false;
			ChangeColor();
		}

		void OnGameOverStarted()
		{
			isGameOver = true;
		}

        /// <summary>
        /// 改变颜色
        /// </summary>
		void ChangeColor(){


			Color colorTemp = colors [UnityEngine.Random.Range (0, colors.Length)];

            //DOColor 吐温类相机写成backgroundColor给定的值。还存储镜头渐变的目标,因此它可以用于过滤操作
            // SetEase 设置渐变的缓解。如果应用于序列简化了整个动画序列
            cam.DOColor(colorTemp,3f).SetEase(Ease.Linear)
                // SetDelay 设置一个延迟启动的渐变。对序列没有影响或者渐变已经开始
                .SetDelay(timeChangeColor)
				.OnComplete(() => {
                    //PlayerPrefs 的扩展
                    // PlayerPrefs 存储和访问玩家游戏会话之间的偏好。
                    PlayerPrefsX.SetColor("BACKGROUND_COLOR", cam.backgroundColor);

					if(!isGameOver)
					{
						ChangeColor();
					}
				});

		}

		void OnApplicationQuit()
		{
            // PlayerPrefs 存储和访问玩家游戏会话之间的偏好。
            PlayerPrefs.Save();
		}
	}
}