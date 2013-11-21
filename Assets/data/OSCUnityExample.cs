using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using UnityEngine;
using Ventuz.OSC;
using System.Collections;

class OSCUnityExample : MonoBehaviour
{
	public int SKELETON_MAX  = 8;
	private int JOINT_MAX = 20;
   	private bool initialized = false;
	private UdpReader OscReader;
  //  private UdpWriter OscWriter;
	private static string[] oscArgs = new string[2];
	private byte[] fullMessage;
   	private byte[] byteReceived;    int total;
	private int count = 1;
	private bool newGesture = false;
	//private bool newData = false;

    private string stringToEdit = "Hello World";
	//public TextMesh texto;
	private SkeletonDataSource skeleton;
	private List<List<GameObject>> skeletonsEsferas;
	private Texture2D tex;
	private Texture2D texRadar;
	public Texture drawTexture;
	private string gesture;
	public GUISkin gSkin;
	public Transform[] player;
	public Texture[] textura;
	public Texture fondo;
	private Shader shader2;
	private Shader shader1;
	private string FPS = "FPS 0";
	private string KINECT_STATUS = "STATUS Disconnect";
	public Font font;
	public Color color;
	public Color [] skeletonColor;
	
    void Start()
    {
		Application.runInBackground = true;
		shader2 = Shader.Find("Transparent/Bumped Diffuse");
		shader1 = Shader.Find("Bumped Diffuse");
		tex = new Texture2D(4, 4);
		texRadar = new Texture2D(1, 1);
		this.gameObject.name = this.gameObject.name + " example OSC ventuz";
        initialized = true;
      	OscReader = new UdpReader(3001);
        oscArgs[0] = "127.0.0.1";
        oscArgs[1] = "3001";
       // OscWriter = new UdpWriter(oscArgs[0], 3002);
		createSkeletonsEsferas();
	
    }
	private void createSkeletonsEsferas(){
		skeletonsEsferas = new List<List<GameObject>>();
		for (int j=0;j<SKELETON_MAX;j++){
			List<GameObject> esferas = new List<GameObject>();
			GUI.color = skeletonColor[j];
			for(int i=0;i<JOINT_MAX;i++){
				GameObject g = GameObject.CreatePrimitive(PrimitiveType.Sphere);
				g.renderer.material.mainTexture = textura[8];
				g.transform.localScale= new Vector3(0.1f,0.1f,0.1f);
				g.renderer.material.shader = shader2;
				esferas.Add(g);		
			}
			skeletonsEsferas.Add(esferas);
		}
	}
	
	
	
    void Update()
    {
        // Wait for initialization
        if (!initialized) return;
        	ParseMessages();
    }

	 void ParseMessages()
    {
        OscMessage message = OscReader.Receive();

        if (message == null) return;
        OscBundle bundle = message as OscBundle;
        if (bundle == null) return;
	
        foreach (OscElement element in bundle.Elements)
        {
	 		FPS = "FPS " +(string)element.Args[5];
			KINECT_STATUS = "STATUS " +(string)element.Args[6];
			
            if (element.Address.Contains("Skeleton"))
            {
				string hands = (string)element.Args[3];
        		string gesto = (string)element.Args[2];      				
				if (gesto != ""){
					gesture = gesto;
					newGesture = true;
					StartCoroutine(waitingAFewSeconds());
				}			
				string dato = (string)element.Args[1];
                string time = (string)element.Args[0];
			    stringToEdit+= string.Format("{0}\r\n",dato);
				skeleton = SkeletonParser.parse(dato);
            }
            
          	if (element.Address.Contains("/video/color"))
            {                          	
				byte[] bytesReceived = (byte[])element.Args[4];   
				//Memory stream to store the bitmap data.
			    System.IO.MemoryStream ms =  new System.IO.MemoryStream(bytesReceived);
			   
			    tex.LoadImage(ms.ToArray());
			    ms.Dispose();						
             }			
         }
	}
	
	 private IEnumerator waitingAFewSeconds()
	{
		// Wait a few seconds for the kick to end and the wall to come crashing down before resetting the game
		yield return new WaitForSeconds(2f);
		newGesture = false;
		gesture = "";
	}
	
	void OnGUI () {

		GUI.skin = gSkin;
		
		int screenW = Screen.width;
		int screenH = Screen.height;		
					
		
		GUI.Box (new Rect (screenW - screenW/5 - 50,screenH - screenH/5 + 30, 310, 100), "");
		GUILayout.BeginArea(new Rect (screenW - screenW/5 - 50,screenH - screenH/5 + 60, 300, 100));
			GUILayout.Label (FPS);		
		GUILayout.EndArea ();
		
		GUI.Box (new Rect (screenW/3 - 50,screenH - screenH/5 + 30, 600, 100), "");		
		GUILayout.BeginArea(new Rect (screenW/3  - 50,screenH - screenH/5 + 60, 600, 100));
			GUILayout.Label (KINECT_STATUS);		
		GUILayout.EndArea ();
		
		GUI.Box (new Rect (40,screenH - screenH/5 + 30, 310, 100), "");
		GUILayout.BeginArea (new Rect(10,screenH - screenH/5 + 60, screenW/3-20, 160));
				GUILayout.Label ("Gesture "+gesture);	
		GUILayout.EndArea ();
	
		if (tex != null){
			 GUI.DrawTexture(new Rect(Screen.width - 128,0,128,128), tex);
		}
		
		GUI.DrawTexture(new Rect(0,0,128,128), texRadar);
		
		if (skeleton != null)
		{
				List<SkeletonsData> Skeletons = skeleton.getSkeletons();
//			Debug.Log("skeleton count " + Skeletons.Count);
				for (int j=0;j<SKELETON_MAX;j++){
					if (j < Skeletons.Count){
						List<JointSkeleton> Joints = Skeletons[j].getSkeletonJoint();
						int i=0;
						foreach(JointSkeleton joint in Joints)	{						
							Vector3 newPosition = new Vector3( joint.getJointPosition().X,joint.getJointPosition().Y,joint.getJointPosition().Z);
							skeletonsEsferas[j][i].transform.position = newPosition;
							skeletonsEsferas[j][i].renderer.material.mainTexture =  textura[j];
							skeletonsEsferas[j][i].renderer.material.shader = shader1;
							i++;	
					
							//draw radar position
							if (joint.getJointName().Equals(JointType.HipCenter.ToString())){
						
								Vector3 com = new Vector3( joint.getJointPosition().X,joint.getJointPosition().Y,joint.getJointPosition().Z);
					            Vector2 radarPosition = new Vector2();
								radarPosition.x = (com.x + 1.0f)/2.0f;
								radarPosition.y = com.z/4.0f;

								radarPosition.x = Mathf.Clamp(radarPosition.x, 0.0f, 1.0f);
					            radarPosition.y = Mathf.Clamp(radarPosition.y, 0.0f, 1.0f);
								GUI.DrawTexture(new Rect(radarPosition.x*128 - 12,radarPosition.y*128 - 12,40,40), drawTexture);							
							}						
						}
					}else{
						for(int i=0;i<JOINT_MAX;i++){
								skeletonsEsferas[j][i].renderer.material.mainTexture =  textura[8];
								skeletonsEsferas[j][i].renderer.material.shader = shader2;
						}
					}
				}
			}
	}
	
	private void setJointPosition(JointSkeleton joint){
		
		string jointName = joint.getJointName();
		Vector3 newPosition = new Vector3( joint.getJointPosition().X,joint.getJointPosition().Y,joint.getJointPosition().Z);
		
		if (jointName.Equals(JointType.HipCenter.ToString())){
			
			player[1].position = newPosition;
		
		}
		
		if (jointName.Equals(JointType.Spine.ToString())){
			player[0].position = newPosition;
			player[20].position = newPosition;
		}
		
		if (jointName.Equals(JointType.ShoulderCenter.ToString()))
			player[2].position = newPosition;
		
		if (jointName.Equals(JointType.Head.ToString()))
			player[3].position = newPosition;
		
		if (jointName.Equals(JointType.ShoulderLeft.ToString()))
			player[4].position = newPosition;
		
		if (jointName.Equals(JointType.ElbowLeft.ToString()))
			player[5].position = newPosition;
		
		if (jointName.Equals(JointType.WristLeft.ToString()))
			player[6].position = newPosition;
		
		if (jointName.Equals(JointType.HandLeft.ToString()))
			player[7].position = newPosition;
		
		if (jointName.Equals(JointType.ShoulderRight.ToString()))
			player[8].position = newPosition;
		
		if (jointName.Equals(JointType.ElbowRight.ToString()))
			player[9].position = newPosition;
		
		if (jointName.Equals(JointType.WristRight.ToString()))
			player[10].position = newPosition;
		
		if (jointName.Equals(JointType.HandRight.ToString()))
			player[11].position = newPosition;
		
		if (jointName.Equals(JointType.HipLeft.ToString()))
			player[12].position = newPosition;
		
		if (jointName.Equals(JointType.KneeLeft.ToString()))
			player[13].position = newPosition;
		
		if (jointName.Equals(JointType.AnkleLeft.ToString()))
			player[14].position = newPosition;
		
		if (jointName.Equals(JointType.FootLeft.ToString()))
			player[15].position = newPosition;
		
		if (jointName.Equals(JointType.HipRight.ToString()))
			player[16].position = newPosition;
		
		if (jointName.Equals(JointType.KneeRight.ToString()))
			player[17].position = newPosition;
		
		if (jointName.Equals(JointType.AnkleRight.ToString()))
			player[18].position = newPosition;
		
		if (jointName.Equals(JointType.FootRight.ToString()))
			player[19].position = newPosition;
	}
	
	private bool isValid(byte[] fullMessage)
    {
        int count = 0;
        for (int i = 0; i < fullMessage.Length; i++) {
            if (count == 50 && i == 50)
                return false;
            if (fullMessage[i] != 0)
                return true;
            else
                count++;
        }
        return false;
        }	
}

