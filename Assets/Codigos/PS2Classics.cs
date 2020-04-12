using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
//using UnityEditor.PS4;
using System.IO;
using System;
using System.Runtime.InteropServices;
using UnityEngine.SceneManagement;
using System.Data;
using DiscUtils.Iso9660;
using System.Linq;

public class PS2Classics : MonoBehaviour {

	[DllImport("universal")]
	private static extern int FreeUnjail(int FWVersion);

	[DllImport("universal")]
	private static extern int FreeFTP();

	[DllImport("universal")]
	private static extern int FreeMount();
	public AudioClip[] clips;
	private AudioSource audiosource;
	public bool randomPlay = false;
	private int currentClipIndex = 0;

	public static PS2Classics instance;

	public Text txtPath;
	public Text DateTime;
	public Text EmptyList;
	private string _s_ = "/";

	private int level = 0;
	private List<Vector2> LastPos = new List<Vector2>();

	public GameObject PanelVideo;
	public Image PanelVideoImagen;
	public GameObject PanelImagen;
	public Image PanelImagenImagen;

	public GameObject carpetasPrefab;
	public GameObject ficherosPrefabMP4;
	public GameObject ficherosPrefabMOV;
	public GameObject ficherosPrefabSRT;
	public GameObject ficherosPrefabFOT;
	public ScrollRect scrollRect;
	public RectTransform contentPanel;
	public Slider miVolumen;
	public Text Subtitulos;

	private List<GameObject> ObjetosCreados = new List<GameObject>();
	private List<string> TODOS = new List<string>();

	private int Posicion = 0;
	private string[] scaneo;
	public List<PS2Items> listofgames = new List<PS2Items>();
	//path to USB0
	public string camino = "/";//= @"G:\Games\Playstation\PS2";
	//path to usb1
	public string usb1 = "/usb1";

	private bool Paso = true;
	private bool EnImagen = false;

	public bool EnVideo = false;
	public bool FullScreen = false;

	public bool DisableDebug =false;

	public enum USBType
	{
		USB0 =0,
		USB1 = 1,
		mntUSB0 =2,
		mntUSB1 = 3,
		None = 9
	}


	public void Awake()
	{
		instance = this;
	}


	// Use this for initialization
	void Start () {
		audiosource = FindObjectOfType<AudioSource>();
		audiosource.loop = false;
		if(!audiosource.isPlaying)
		{

			audiosource.clip = GetRandomClip();
			audiosource.Play();
		}


		System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-US");
		DateTime.text = System.DateTime.Now.AddHours(-3).ToShortTimeString() + "\n" + System.DateTime.Now.AddHours(-3).ToLongDateString() + "\n" + SystemInfo.operatingSystem.ToString();
		//StartCoroutine(ActualizarFechaHora());
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	private AudioClip GetRandomClip()
	{
		int item = UnityEngine.Random.Range (0, clips.Length);
		currentClipIndex = item;
		return clips[item];
	}
	private AudioClip GetNextClip()
	{
		int item = ((currentClipIndex + 1) % clips.Length);
		currentClipIndex = item;
		return clips[item];
	}


	#region << PS2 Items >>

	public string GetNameFromID(string PS2ID,DataTable dt)
	{
		try {
			for (int i = 0; i < dt.Rows.Count; i++) {
				if (dt.Rows [i] ["PS2ID"].ToString () == PS2ID) {
					return dt.Rows [i] ["PS2Title"].ToString ();
				}
			}
		} catch (Exception ex) {

		}
		return "";
	}

	public string GetRegionFromID(string PS2ID,DataTable dt)
	{
		try {
			for (int i = 0; i < dt.Rows.Count; i++) {
				if (dt.Rows [i] ["PS2ID"].ToString () == PS2ID) {
					return dt.Rows [i] ["Region"].ToString ();
				}
			}
		} catch (Exception ex) {

		}
		return "";
	}

	//PS2 Images
	public string GetImageFromID(string PS2ID,DataTable dt)
	{
		try {
			for (int i = 0; i < dt.Rows.Count; i++) {
				if (dt.Rows [i] ["PS2ID"].ToString () == PS2ID) {
					return dt.Rows [i] ["Imagurl"].ToString ();
				}
			}
		} catch (Exception ex) {
			return "";
		}
		return "";
	}

	public string GetPS2ID(string isopath)
	{
		using (FileStream isoStream = System.IO.File.OpenRead(isopath))
		{
			try{
				//use disk utils to read iso file
				CDReader cd = new CDReader(isoStream, true);
				//look for the specific file naimly the system config file
				Stream fileStream = cd.OpenFile(@"SYSTEM.CNF", FileMode.Open);
				// Use fileStream...
				TextReader tr = new StreamReader(fileStream);
				string fullstring = tr.ReadToEnd();//read string to end this will read all the info we need

				//mine for info
				string Is = @"\";
				string Ie = ";";

				//mine the start and end of the string
				int start = fullstring.ToString().IndexOf(Is) + Is.Length;
				int end = fullstring.ToString().IndexOf(Ie, start);
				if (end > start)
				{
					string PS2Id = fullstring.ToString().Substring(start, end - start);

					if (PS2Id != string.Empty)
					{
						return PS2Id.Replace(".", "").Replace("_","-");
						Console.WriteLine("PS2 ID Found" + PS2Id);
					}
					else
					{
						Console.WriteLine("Could not load PS2 ID");
						return "NOT FOUND";
					}
				}
			}
			catch(Exception ex) {
				return ex.Message;
			}

		}
		return "UTIL BROKE";
	}


	public DataTable ConvertToDataTable (StreamReader sr)
	{
		DataTable tbl = new DataTable();

		//add defualt columns 
		tbl.Columns.Add("PS2Title");
		tbl.Columns.Add("Region");
		tbl.Columns.Add("PS2ID");
		tbl.Columns.Add ("Imagurl");

		string line;
		while ((line = sr.ReadLine ()) != null) 
		{


			var cols = line.Split (';');
			if (cols.Count() > 2) 
			{
				DataRow dr = tbl.NewRow ();
				dr [0] = cols [0];
				dr [1] = cols [1];
				dr [2] = cols [2];
				if (cols.Count() > 3) {
					dr [3] = cols [3];
				}
				tbl.Rows.Add (dr);

			}
		}

		return tbl;
	}

	#endregion << PS2 Items >>

	public class PS2Items
	{
		public string PS2ID { get; set; }
		public string PS2_Title { get; set; }
		public string Region { get; set;}
		public string Picture { get; set; }
		public string Path { get; set; }
	}

}
