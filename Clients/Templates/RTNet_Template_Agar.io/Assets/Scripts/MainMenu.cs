using System;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using RTNet;

[RequireComponent(typeof(RTNetView))]
public class MainMenu : MonoBehaviour
{
	public enum MenuStates { Main = 0, Connect = 1, Settings = 2, Credits = 3, Quitting = 4, InGame = 5, Connecting = 6 }
	public enum ButtonPress { Back = 0, MenuConnectButton = 1, MenuSettingsButton = 2, MenuCreditsButton = 3, MenuQuitButton = 4 }

	[Header("Assignables")]
	public InputField addressText;
	public InputField portText;

	[Space()]

	public GameObject mainMenu;
	public GameObject connectMenu;
	public GameObject settingsMenu;
	public GameObject creditsMenu;
	public GameObject ingameUI;

	[Space()]
	public Button btnBack;
	public Button btnConnect;

	[Header("Client Options")]
	public int bufferSize = 512;
	public bool debugMode = false;

	public MenuStates menuState = MenuStates.Main;

	void Start()
	{
		GetComponent<RTNetView>().DebugMode = debugMode;
		GetComponent<RTNetView>().BufferSize = bufferSize;

		ShowMenu(MenuStates.Main);
	}

	public void ButtonPressed(int i)
	{
		if ((ButtonPress)i == ButtonPress.MenuQuitButton)
		{
#if UNITY_EDITOR
			UnityEditor.EditorApplication.isPlaying = false;
#else
			Application.Quit();
#endif
		}
		ShowMenu((MenuStates)i);
	}

	private void ShowMenu(MenuStates state)
	{
		if(mainMenu != null)
			mainMenu.SetActive(state == MenuStates.Main);
		if (connectMenu != null)
			connectMenu.SetActive(state == MenuStates.Connect);
		if (settingsMenu != null)
			settingsMenu.SetActive(state == MenuStates.Settings);
		if (creditsMenu != null)
			creditsMenu.SetActive(state == MenuStates.Credits);
		if (ingameUI != null)
			ingameUI.SetActive(state == MenuStates.InGame);
		if(btnBack != null)
			btnBack.gameObject.SetActive(state != MenuStates.Main && state != MenuStates.Quitting && state != MenuStates.InGame && state != MenuStates.Connecting);
		menuState = state;
	}

	void Update()
	{
		if(menuState == MenuStates.Connecting)
		{
			if(GetComponent<RTNetView>().Connected)
			{
				ShowMenu(MenuStates.InGame);
				GetComponent<GameManager>().state = GameManager.PlayerState.Dead;
			}
		}
	}

	public void Connect()
	{
		if(addressText == null || portText == null)
		{
			Debug.LogError("MainMenu 'AddressText' or 'PortText' not assigned");
			return;
		}
		string ip = null;
		int port = -1;
		try
		{
			if (string.IsNullOrEmpty(addressText.text))
				ip = "127.0.0.1";
			else
				ip = System.Net.IPAddress.Parse(addressText.text).ToString();
			if (string.IsNullOrEmpty(portText.text))
				port = 4434;
			else
				port = int.Parse(portText.text);
		}
		catch (Exception e)
		{
			if (string.IsNullOrEmpty(ip))
				Debug.LogWarning("IP Address is invalid");
			else if (port == -1)
				Debug.LogWarning("Invalid port");
			else
				Debug.LogException(e);
			return;
		}
		addressText.interactable = false;
		portText.interactable = false;
		btnBack.interactable = false;
		btnConnect.interactable = false;

		menuState = MenuStates.Connecting;
		GetComponent<RTNetView>().Connect(ip, port);
	}

	public void Disconnect()
	{
		GetComponent<RTNetView>().Disconnect();
	}
}
