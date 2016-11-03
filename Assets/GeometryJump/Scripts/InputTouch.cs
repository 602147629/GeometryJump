/***********************************************************************************************************
 * Produced by App Advisory - http://app-advisory.com													   *
 * Facebook: https://facebook.com/appadvisory															   *
 * Contact us: https://appadvisory.zendesk.com/hc/en-us/requests/new									   *
 * App Advisory Unity Asset Store catalog: http://u3d.as/9cs											   *
 * Developed by Gilbert Anthony Barouch - https://www.linkedin.com/in/ganbarouch                           *
 ***********************************************************************************************************/




using UnityEngine;
using System.Collections;

/// <summary>
/// Class in charge to manage input touch and desktop input in the game
/// 负责管理类输入触摸和桌面游戏
/// </summary>
namespace AppAdvisory.GeometryJump
{
	public class InputTouch : MonoBehaviour
	{
        // 触摸左边的代理
		public delegate void TouchLeft();
        //触摸左边的事件
		public static event TouchLeft OnTouchLeft;

        //触摸右边的代理
		public delegate void TouchRight();
        //触摸右边的事件
		public static event TouchRight OnTouchRight;

        //触摸屏幕的代理
		public delegate void TouchScreen();
        //触摸屏幕的事件
		public static event TouchScreen OnTouchScreen;

		#if UNITY_TVOS
		private Vector2 startPosition;
		void OnEnable()
		{
		GameManager.OnGameStart += OnGameStart;

		GameManager.OnGameOver += OnGameOver;
		}
		void OnDisable()
		{
		GameManager.OnGameStart -= OnGameStart;

		GameManager.OnGameOver -= OnGameOver;
		}

		bool gameStarted = true;

		void OnGameStart()
		{
		UnityEngine.Apple.TV.Remote.touchesEnabled = true;
		UnityEngine.Apple.TV.Remote.allowExitToHome = false;

		gameStarted = true;
		}

		void OnGameOver()
		{
		UnityEngine.Apple.TV.Remote.touchesEnabled = false;
		UnityEngine.Apple.TV.Remote.allowExitToHome = true;

		gameStarted = false;
		}

		void Start()
		{
		UnityEngine.Apple.TV.Remote.reportAbsoluteDpadValues = true;
		}


		#endif

		void Update () 
		{



			#if UNITY_TVOS

			if(!gameStarted)
			{
			return;
			}

			float h = Input.GetAxis("Horizontal");

			if(h < 0)
			{
			_OnTouchLeft();
			}
			else if(h > 0)
			{
			_OnTouchRight();
			}

			int nbTouches = Input.touchCount;

			if(nbTouches > 0)
			{

			Touch touch = Input.GetTouch(0);

			TouchPhase phase = touch.phase;

			print("" + phase.ToString() + " position = " + touch.position);

			}

			#endif

			#if (UNITY_ANDROID || UNITY_IOS || UNITY_TVOS) 
			int nbTouches = Input.touchCount;

			if(nbTouches > 0)
			{

				Touch touch = Input.GetTouch(0);

				TouchPhase phase = touch.phase;

				if (phase == TouchPhase.Began)
				{

					if (touch.position.x < Screen.width / 2f)
					{
						_OnTouchLeft();
					}
					else
					{
						_OnTouchRight();
					}


				}

			}

			#endif

			#if (!UNITY_ANDROID && !UNITY_IOS && !UNITY_TVOS) || UNITY_EDITOR

			if (Input.GetKeyDown (KeyCode.LeftArrow))
			{
				_OnTouchLeft();
			}
			if (Input.GetKeyDown (KeyCode.RightArrow))
			{
				_OnTouchRight();
			}

			#endif
		}

		void _OnTouchLeft()
		{
			if(OnTouchScreen != null)
				OnTouchScreen();

			if(OnTouchLeft != null)
				OnTouchLeft();
		}

		void _OnTouchRight()
		{
			if(OnTouchScreen != null)
				OnTouchScreen();

			if(OnTouchRight != null)
				OnTouchRight();
		}
	}
}