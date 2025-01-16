using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using StarkSDKSpace;
using TTSDK.UNBridgeLib.LitJson;
using TTSDK;

public class GameManager : MonoBehaviour {

	enum State{
		START,
		PLAY,
		GAMEOVER,
	}

	public static float time;
	public float timeLimit = 30;
	public static bool isAddTime;
	const float waitTime = 8;
	public Button startGameBtn;

	Animator anim;
	MoleManager moleManager;
	TMP_Text remainingTIme;
	AudioSource audio;

	private StarkAdManager starkAdManager;
	public string clickid;

	State state;
	float timer;
	bool isOver;

	void Start () 
	{

		Application.targetFrameRate = 60;
		if(isAddTime && PlayerPrefs.HasKey("LastScore"))
        {
			ScoreManager.instance.curSocre = PlayerPrefs.GetInt("LastScore");
        }
		if (!PlayerPrefs.HasKey("MaxScore"))
        {
			PlayerPrefs.SetInt("MaxScore", ScoreManager.instance.maxScore);
		}
		startGameBtn.onClick.AddListener(StartGame);
		this.state = State.START;
		this.timer = 0;
		this.anim = GameObject.Find ("Canvas").GetComponent<Animator> ();
		this.moleManager = GameObject.Find ("GameManager").GetComponent<MoleManager> ();
		this.remainingTIme = GameObject.Find ("RemainingTime").GetComponent<TMP_Text>();
		this.audio = GetComponent<AudioSource> ();
	}
	
	void Update () 
	{
		if (this.state == State.PLAY) 
		{
			isOver = false;
			this.timer += Time.deltaTime;
			if (isAddTime)
			{
				timeLimit = 16;
			}
			else
            {
				timeLimit = 30;

			}
			time = this.timer / timeLimit;
			if (this.timer > timeLimit) 
			{				
				this.state = State.GAMEOVER;

				// show gameover label
				this.anim.SetTrigger ("GameOverTrigger");

				// stop to generate moles
				this.moleManager.StopGenerate ();

				this.timer = 0;

				// stop audio
				this.audio.loop = false;
				isAddTime = false;
			}

			this.remainingTIme.text = "剩余时间: " + ((int)(timeLimit-timer)).ToString ("D2");
		}
		else if (this.state == State.GAMEOVER) 
		{
			ScoreManager.instance.SetEndScoreView(ScoreManager.instance.curSocre);
			ScoreManager.instance.SetMaxScoreView(PlayerPrefs.GetInt("MaxScore"));
			if(ScoreManager.instance.curSocre>PlayerPrefs.GetInt("MaxScore"))
            {
				PlayerPrefs.SetInt("MaxScore", ScoreManager.instance.curSocre);
				ScoreManager.instance.maxScore= PlayerPrefs.GetInt("MaxScore");
			}
			this.timer += Time.deltaTime;
			if(isAddTime)
            {
				this.state = State.PLAY;
				return;
			}
			if (this.timer > waitTime) 
			{
				SceneManager.LoadScene ( SceneManager.GetActiveScene().name );
			}

			this.remainingTIme.text = "";
			PlayerPrefs.Save();

			if (!isOver)
			{
				isOver = true;
				ShowInterstitialAd("3989k25lh3g02kk9b8",
			   () =>
			   {

			   },
			   (it, str) =>
			   {
				   Debug.LogError("Error->" + str);
			   });
			}
		}
	}

	/// <summary>
	/// 播放插屏广告
	/// </summary>
	/// <param name="adId"></param>
	/// <param name="errorCallBack"></param>
	/// <param name="closeCallBack"></param>
	public void ShowInterstitialAd(string adId, System.Action closeCallBack, System.Action<int, string> errorCallBack)
	{
		starkAdManager = StarkSDK.API.GetStarkAdManager();
		if (starkAdManager != null)
		{
			var mInterstitialAd = starkAdManager.CreateInterstitialAd(adId, errorCallBack, closeCallBack);
			mInterstitialAd.Load();
			mInterstitialAd.Show();
		}
	}

	void StartGame()
    {
		if(state==State.START)
        {
			this.state = State.PLAY;

			// hide start label
			this.anim.SetTrigger("StartTrigger");

			// start to generate moles
			this.moleManager.StartGenerate();

			this.audio.Play();
		}
	}

	public void AddTime()
	{
		ShowVideoAd("lnmk284ln3j3l8tc4l",
			(bol) => {
				if (bol)
				{
					isAddTime = true;
					PlayerPrefs.SetInt("LastScore", ScoreManager.instance.curSocre);
					SceneManager.LoadScene(SceneManager.GetActiveScene().name);

					clickid = "";
					getClickid();
					apiSend("game_addiction", clickid);
					apiSend("lt_roi", clickid);
				}
				else
				{
					StarkSDKSpace.AndroidUIManager.ShowToast("观看完整视频才能获取奖励哦！");
				}
			},
			(it, str) => {
				Debug.LogError("Error->" + str);
				//AndroidUIManager.ShowToast("广告加载异常，请重新看广告！");
			});

	}

	public void ShowVideoAd(string adId, System.Action<bool> closeCallBack, System.Action<int, string> errorCallBack)
	{
		starkAdManager = StarkSDK.API.GetStarkAdManager();
		if (starkAdManager != null)
		{
			starkAdManager.ShowVideoAdWithId(adId, closeCallBack, errorCallBack);
		}
	}

	public void getClickid()
	{
		var launchOpt = StarkSDK.API.GetLaunchOptionsSync();
		if (launchOpt.Query != null)
		{
			foreach (KeyValuePair<string, string> kv in launchOpt.Query)
				if (kv.Value != null)
				{
					Debug.Log(kv.Key + "<-参数-> " + kv.Value);
					if (kv.Key.ToString() == "clickid")
					{
						clickid = kv.Value.ToString();
					}
				}
				else
				{
					Debug.Log(kv.Key + "<-参数-> " + "null ");
				}
		}
	}

	public void apiSend(string eventname, string clickid)
	{
		TTRequest.InnerOptions options = new TTRequest.InnerOptions();
		options.Header["content-type"] = "application/json";
		options.Method = "POST";

		JsonData data1 = new JsonData();

		data1["event_type"] = eventname;
		data1["context"] = new JsonData();
		data1["context"]["ad"] = new JsonData();
		data1["context"]["ad"]["callback"] = clickid;

		Debug.Log("<-data1-> " + data1.ToJson());

		options.Data = data1.ToJson();

		TT.Request("https://analytics.oceanengine.com/api/v2/conversion", options,
		   response => { Debug.Log(response); },
		   response => { Debug.Log(response); });
	}



}
