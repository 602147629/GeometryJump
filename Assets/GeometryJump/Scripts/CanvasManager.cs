/***********************************************************************************************************
 * Produced by App Advisory - http://app-advisory.com													   *
 * Facebook: https://facebook.com/appadvisory															   *
 * Contact us: https://appadvisory.zendesk.com/hc/en-us/requests/new									   *
 * App Advisory Unity Asset Store catalog: http://u3d.as/9cs											   *
 * Developed by Gilbert Anthony Barouch - https://www.linkedin.com/in/ganbarouch                           *
 ***********************************************************************************************************/




#pragma warning disable 0618 

using UnityEngine;
using System;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using DG.Tweening;
#if UNITY_5_3_OR_NEWER
using UnityEngine.SceneManagement;
#endif

#if APPADVISORY_ADS
using AppAdvisory.Ads;
#endif

/// <summary>
/// Class in charge to manage all the UI in the game
/// </summary>
namespace AppAdvisory.GeometryJump
{
	public class CanvasManager : MonoBehaviorHelper 
	{
		/// <summary>
		/// We show ads - interstitials - ever 10 game over by default. To change it, change this number. You have to get "Very Simple Ad" from the asset store to use it: http://u3d.as/oWD
		/// </summary>
		public int numberOfPlayToShowInterstitial = 5;

		public string VerySimpleAdsURL = "http://u3d.as/oWD";
        //SerializeField 统一序列化一个私有字段。
        [SerializeField] private Text scoreText; 
		[SerializeField] private Text bestScoreText; 
		[SerializeField] private Text lastScoreText; 

        // 在游戏中
		[SerializeField] private CanvasGroup canvasGroupInstruction;
        // 分数
		[SerializeField] private CanvasGroup canvasGroupScore;
        // 游戏结束
		[SerializeField] private CanvasGroup canvasGroupGameOver;
        // 任务商店
		[SerializeField] private CanvasGroup canvasGroupMaskShop;

		[SerializeField] private Text lifeText;
        //钻石
		[SerializeField] private Text diamondText;

        // 继续 有命
		[SerializeField] private Button buttonContinueWithLife;
        // 继续 用钻石
        [SerializeField] private Button buttonContinueWithDiamond;
        //获取免费的生命
		[SerializeField] private Button buttonGetFreeLife;
        // 免费的钻石
		[SerializeField] private Button buttonGetFreeDiamonds;
        // 重新开始
		[SerializeField] private Button buttonRestart;
        //
		[SerializeField] private Button buttonCloseMask;

        // AnimationTransitionManager类来管理过渡动画
        public AnimationTransitionManager m_animationTransitionManager;
        // AnimationTransition 动画过渡以及窗口启动和重启
        public AnimationTransition m_animationTransition;

        // 动画启动的代理
		public delegate void AnimationTransitionInStart();
        // 动画开始的事件
		public static event AnimationTransitionInStart OnAnimationTransitionInStart;

        // 动画结束的代理
        public delegate void AnimationTransitionInEnd();
        // 动画结束的事件
        public static event AnimationTransitionInEnd OnAnimationTransitionInEnd;

		public delegate void AnimationTransitionOutStart();
		public static event AnimationTransitionOutStart OnAnimationTransitionOutStart;

		public delegate void AnimationTransitionOutEnd();
		public static event AnimationTransitionOutEnd OnAnimationTransitionOutEnd;

		public void _AnimationTransitionInStart()
		{
			if(OnAnimationTransitionInStart != null)
				OnAnimationTransitionInStart();
		}
		public void _AnimationTransitionInEnd()
		{
			if(OnAnimationTransitionInStart != null)
				OnAnimationTransitionInEnd();
		}
		public void _AnimationTransitionOutStart()
		{
			if(OnAnimationTransitionOutStart != null)
				OnAnimationTransitionOutStart();
		}
		public void _AnimationTransitionOutEnd()
		{
			if(OnAnimationTransitionOutEnd != null)
				OnAnimationTransitionOutEnd();
		}


		float timeAlphaAnim = 1f;

        // 结构中的透明度
		float alphaInstruction
		{
			get
			{
				return canvasGroupInstruction.alpha;
			}

			set
			{
				canvasGroupInstruction.alpha = value;
				//			canvasGroupScore.alpha = 1f - value;
			}
		}

		void OnEnable()
		{
			GameManager.OnSetPoint += SetPointText;

			GameManager.OnSetDiamond += SetDiamondText;

			GameManager.OnGameStart += OnGameStart;

			GameManager.OnGameOverEnded += OnGameOverEnded;
		}

		void OnDisable()
		{
			GameManager.OnSetPoint -= SetPointText;

			GameManager.OnSetDiamond -= SetDiamondText;

			GameManager.OnGameStart -= OnGameStart;

			GameManager.OnGameOverEnded -= OnGameOverEnded;
		}

		void Start()
		{
            // 给Best的Text赋值
			SetBestScoreText(gameManager.GestBestScore());

			SetPointText(0);

            //给last 的 Text 赋值 
			lastScoreText.text = "Last: " + gameManager.GestLastScore();

            //生命
			lifeText.text = "x " + gameManager.GetLife().ToString();

			alphaInstruction = 1f;

            //游戏结束
			canvasGroupGameOver.alpha = 0;

            // 商店 任务
			canvasGroupMaskShop.alpha = 0;

			canvasGroupGameOver.gameObject.SetActive(false);

            // 钻石
			SetDiamondText(gameManager.diamond);
		}

        //按钮的逻辑
		void ButtonLogic()
		{
            //是否初始化广告
			bool adsInitialized = false;

			#if APPADVISORY_ADS
			adsInitialized = AdsManager.instance.IsReadyRewardedVideo ();
			#endif

            //是否有生命
			bool haveLife = gameManager.HaveLife();
            //是否有足够的钻石
			bool haveEnoughtDiamond = gameManager.diamond >= 100;

			ActivateButton(buttonContinueWithLife, haveLife);
			ActivateButton(buttonGetFreeLife, adsInitialized);
			ActivateButton(buttonGetFreeDiamonds, adsInitialized);
			ActivateButton(buttonContinueWithDiamond, haveEnoughtDiamond);
		}

        //按钮是否可操作
		void ActivateButton(Button b, bool activate)
		{
			b.GetComponent<CanvasGroup>().alpha = activate ? 1 : 0.5f;
			b.interactable = activate;
		}


        //游戏结束后
		void OnGameOverEnded()
		{
			ButtonLogic();

			canvasGroupGameOver.alpha = 0;

			canvasGroupGameOver.gameObject.SetActive(true);

            // CanvasGroup α颜色给定的值。也存储canvasGroup渐变的目标,因此它可以用于过滤操作
            // Lambda表达式
            canvasGroupGameOver.DOFade(1,timeAlphaAnim)
				.OnComplete(() => {
					AddButtonListener();
				});

			#if !(UNITY_ANDROID || UNITY_IOS) && UNITY_TVOS
			for(int i = 0; i < tc.childCount; i++)
			{
			var t = tc.GetChild(i);
			if(tc.gameObject.activeInHierarchy)
			{
			var es = FindObjectOfType<EventSystem>();
			es.firstSelectedGameObject = t.gameObject;
			es.SetSelectedGameObject(t.gameObject);

			print("set selected: " + t.name);
			break;
			}
			}
			#endif
		}

		void OnGameStart()
		{
			//		SetCanvasGroupInstructionAlpha(1,0);
			//
			//		canvasGroupInstruction.GetComponent<AnimButtonHierarchy>().DoAnimOut();
		}

        //
		void SetPointText(int point)
		{
            //分数
			scoreText.text = point.ToString();

			int best = gameManager.GestBestScore ();

			if (point > best)
                //Best
				SetBestScoreText(point);
		}

		void SetDiamondText(int diamond)
		{
			diamondText.text = "x " + diamond.ToString();
		}

		void SetBestScoreText(int p)
		{
			bestScoreText.text = "best: " + p.ToString();
		}

        // 设置 CanvasGroup 在游戏中的透明度
        public void SetCanvasGroupInstructionAlpha(float fromA, float toA)
		{
            //DOVirtual 创建虚拟吐温类（两者之间）,可用于改变其他元素通过OnUpdate调用
            DOVirtual.Float(fromA, toA, timeAlphaAnim, (float f) => {
				alphaInstruction = f;
			})
				.OnComplete(() => {
					alphaInstruction = toA;

					if(toA == 0)
					{
						canvasGroupGameOver.gameObject.SetActive(false);
					}
				});
		}

        // 设置CanvasGroup在游戏结束后的透明度
        public void SetCanvasGroupGameOverAlpha(float fromA, float toA)
		{
			DOVirtual.Float(fromA, toA, timeAlphaAnim, (float f) => {
				canvasGroupGameOver.alpha = f;
			})
				.OnComplete(() => {
					canvasGroupGameOver.alpha = toA;

					if(toA == 0)
					{
						canvasGroupGameOver.gameObject.SetActive(false);
					}

					AddButtonListener();
				});
		}

        //按钮的监听
		void AddButtonListener()
		{
			bool adsInitialized = false;

			#if APPADVISORY_ADS
			adsInitialized = AdsManager.instance.IsReadyRewardedVideo ();
			#endif

			bool haveLife = gameManager.HaveLife();
			bool haveEnoughtDiamond = gameManager.diamond >= 100;


			ActivateButton(buttonContinueWithLife, haveLife);
			ActivateButton(buttonGetFreeLife, adsInitialized);
			ActivateButton(buttonGetFreeDiamonds, adsInitialized);
			ActivateButton(buttonContinueWithDiamond, haveEnoughtDiamond);

            //UI.Selectable.interactable。
            buttonRestart.interactable = true;
		}

        // 移除监听
		void RemoveListener()
		{
			buttonContinueWithLife.interactable = false;
			buttonGetFreeLife.interactable = false;
			buttonGetFreeDiamonds.interactable = false;
			buttonContinueWithDiamond.interactable = false;
			buttonRestart.interactable = false;
		}

        //按钮的点击事件
		public void OnClickedButton(GameObject b)
		{
			if(!b.GetComponent<Button>().interactable)
				return;

			if (b.name.Contains("ContinueWithLife"))
				OnClickedContinueWithLife();
			else if (b.name.Contains("ContinueWithDiamond"))
				OnClickedContinueWithDiamond();
			else if (b.name.Contains("ButtonGet3Life"))
				OnClickedGetFreeLife();
			else if (b.name.Contains("ButtonGet300Diamonds"))
				OnClickedGetFreeDiamonds();
			else if (b.name.Contains("GameOver"))
				OnClickedRestart();
			else if (b.name.Contains("ButtonMask"))
				OnClickedButtonMask();
		}

		void OnClickedButtonMask()
		{
			canvasGroupMaskShop.alpha = 0;
			canvasGroupMaskShop.gameObject.SetActive(true);

			canvasGroupMaskShop.DOFade(1,1);

			canvasGroupMaskShop.interactable = true;
			canvasGroupMaskShop.blocksRaycasts = true;

			buttonCloseMask.onClick.AddListener(CloseButtonMask);
		}

		void CloseButtonMask()
		{
			canvasGroupMaskShop.DOFade(0,1)
				.OnComplete( () => {
					canvasGroupMaskShop.blocksRaycasts = false;
					canvasGroupMaskShop.alpha = 0;
					canvasGroupMaskShop.gameObject.SetActive(false);
				});

			canvasGroupMaskShop.interactable = false;
		}

		void OnClickedContinueWithLife()
		{
			RemoveListener();
			SetCanvasGroupGameOverAlpha(1,0);
			gameManager.AddLife(-1);
			lifeText.text = "x " + gameManager.GetLife().ToString();
			playerManager.Continue();
		}

		void OnClickedContinueWithDiamond()
		{
			RemoveListener();
			SetCanvasGroupGameOverAlpha(1,0);
			gameManager.diamond -= 100;
			playerManager.Continue();
		}

		void OnClickedGetFreeLife()
		{
			RemoveListener();

			#if APPADVISORY_ADS
			AdsManager.instance.ShowRewardedVideo((bool success) => {
				if(success)
				{
					gameManager.AddLife(3);
					lifeText.text = "x " + gameManager.GetLife().ToString();
				}
				ButtonLogic();
				AddButtonListener();
				buttonGetFreeLife.gameObject.SetActive(false);
			});
			#endif
		}

		void OnClickedGetFreeDiamonds()
		{
			RemoveListener();

			#if APPADVISORY_ADS
			AdsManager.instance.ShowRewardedVideo((bool success) => {
				if(success)
				{
					gameManager.diamond += 300;
					diamondText.text = "x " + gameManager.diamond.ToString();
				}
				ButtonLogic();
				AddButtonListener();
				buttonGetFreeDiamonds.gameObject.SetActive(false);
			});
			#endif
		}

		/// <summary>
		/// If using Very Simple Ads by App Advisory, show an interstitial if number of play > numberOfPlayToShowInterstitial: http://u3d.as/oWD
		/// </summary>
		public void ShowAds()
		{
            //返回值对应键的首选项如果文件是存在的
            int count = PlayerPrefs.GetInt("GAMEOVER_COUNT",0);
			count++;
            //设置首选项的值根据对应的键的值。
			PlayerPrefs.SetInt("GAMEOVER_COUNT",count);
            //PlayerPrefs 存储和访问玩家游戏会话之间的偏好。
			PlayerPrefs.Save();

			#if APPADVISORY_ADS
			if(count > numberOfPlayToShowInterstitial)
			{
				print("count = " + count + " > numberOfPlayToShowINterstitial = " + numberOfPlayToShowInterstitial);

				if(AdsManager.instance.IsReadyInterstitial())
				{
					print("AdsManager.instance.IsReadyInterstitial() == true ----> SO ====> set count = 0 AND show interstial");
					AdsManager.instance.ShowInterstitial();
					PlayerPrefs.SetInt("GAMEOVER_COUNT",0);
				}
				else
				{
			#if UNITY_EDITOR
					print("AdsManager.instance.IsReadyInterstitial() == false");
			#endif
				}

			}
			else
			{
				PlayerPrefs.SetInt("GAMEOVER_COUNT", count);
			}
			PlayerPrefs.Save();
			#else
			if(count >= numberOfPlayToShowInterstitial)
			{
			Debug.LogWarning("To show ads, please have a look to Very Simple Ad on the Asset Store, or go to this link: " + VerySimpleAdsURL);
			Debug.LogWarning("Very Simple Ad is already implemented in this asset");
			Debug.LogWarning("Just import the package and you are ready to use it and monetize your game!");
			Debug.LogWarning("Very Simple Ad : " + VerySimpleAdsURL);
			PlayerPrefs.SetInt("GAMEOVER_COUNT",0);
			}
			else
			{
			PlayerPrefs.SetInt("GAMEOVER_COUNT", count);
			}
			PlayerPrefs.Save();
			#endif
		}

		public void OnClickedRestart()
		{
			var an = FindObjectsOfType<AnimButtonHierarchy>();

			foreach(var a in an)
			{
				if(a.gameObject.activeInHierarchy)
					a.DoAnimOut();
			}

			DOTween.KillAll();

			ShowAds();

			m_animationTransition.DoAnimationIn( () => {
				RemoveListener();
				StopAllCoroutines();
				PlayerPrefsX.SetColor("BACKGROUND_COLOR", cam.backgroundColor);
				PlayerPrefs.Save();

				#if UNITY_5_3_OR_NEWER
				DOTween.KillAll();

				GC.Collect();

				Resources.UnloadUnusedAssets();

				SceneManager.LoadSceneAsync(0,LoadSceneMode.Single);

				Resources.UnloadUnusedAssets();

				GC.Collect();
				#else
				Application.LoadLevel (Application.loadedLevel);
				#endif
			});

		}
	}
}