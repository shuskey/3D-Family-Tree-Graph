using Assets.Scripts.DataObjects;
using Assets.Scripts.Enums;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class PersonNode : MonoBehaviour
{
    private Person person;
    private PersonDetailsHandler personDetailsHandlerScript;
    private GlobalSpringType globalSpringType;
    
    public float lifeSpan;
    public int birthDate;
    public int endOfPlatformDate;
    public bool isLiving;
    public PersonGenderType personGender;
    public int dataBaseOwnerID;
    public int arrayIndex;
    public (int original, int updated) birthDateQuality;
    public (int original, int updated) deathDateQuality;
    public string dateQualityInformationString = "";

    private bool debugAddMotion = false;
    private Rigidbody myRigidbody;

    GameObject birthConnectionPrefabObject;
    GameObject marriageConnectionPrefabObject;

    float marriageConnectionXScale;
    GameObject bubblePrefabObject;
    GameObject parentPlatformBirthBubble;
    GameObject childPlatformReturnToParent;
    GameObject parentBirthConnectionPoint;
    GameObject returnToMotherBirthConnectionPoint;
    GameObject returnToFatherBirthConnectionPoint;
    GameObject childBirthConnectionPoint;

    const int ScaleThisChildIndex = 0;
    static Color clearWhite = new Color(1.0f, 1.0f, 1.0f, 0.2f);
    static Color pink = new Color(0.8f, 0.5f, 0.8f, 0.7f);
    static Color blue = new Color(0.4f, 0.7f, 0.9f, 0.7f);

    private Color[] personGenderCapsuleBubbleColors = {
            clearWhite,   // notset
            blue,   // male
            pink    // female
        };
    private Color[] personDateQualityColors = {
            new Color(0.4f, 0.4f, 0.4f, 0.5f),   // date = orig
            new Color(0.9f, 0.1f, 0.1f, 0.5f)    // date != orig
        };

    private Color[] personGenderPlatformColors = {
            new Color(0.2f, 0.2f, 0.2f),   // notset
            new Color(0.3f, 0.6f, 0.9f),   // male
            new Color(0.7f, 0.4f, 0.9f)    // female
        };
    private Color[] childRelationshipColors = {
            new Color(0.9f, 0.9f, 0.9f, 0.9f),   // biological
            new Color(0.0f, 0.0f, 0.0f, 0.9f)    // adopted
        };

    void Start()
    {        
        var bothTextMeshProItems = transform.GetComponentsInChildren<TextMeshPro>();
        foreach (var textMeshProItem in bothTextMeshProItems)
        {
            textMeshProItem.text = name;
        }
        GameObject[] personDetailsPanel = GameObject.FindGameObjectsWithTag("PersonDetailsPanel");
        personDetailsHandlerScript = personDetailsPanel[0].transform.GetComponent<PersonDetailsHandler>();
    }

    void Update()
    {
        if (debugAddMotion)
        {
            myRigidbody = this.transform.GetComponent<Rigidbody>();
            var vVelocityDirection = new Vector3(-1, 0, 0);
            myRigidbody.velocity = vVelocityDirection * 1f;
        }
    }

    public void UpdatePersonDetailsWithThisPerson(int currentDate)
    {
        personDetailsHandlerScript.DisplayThisPerson(person, currentDate);
    }

    public void ClearPersonDetails()
    {
        personDetailsHandlerScript.DisplayThisPerson(null);
    }

    public void SetEdgePrefab(GameObject birthConnectionPrefab, GameObject marriageConnectionPrefab, GameObject bubble, GameObject parentPlatformBirthBubble, GameObject childPlatformReturnToParent, float edgeXScale)
    {
        this.birthConnectionPrefabObject = birthConnectionPrefab;
        this.marriageConnectionPrefabObject = marriageConnectionPrefab;
        this.marriageConnectionXScale = edgeXScale;
        this.bubblePrefabObject = bubble;
        this.parentPlatformBirthBubble = parentPlatformBirthBubble;
        this.childPlatformReturnToParent = childPlatformReturnToParent;
    }

    public void SetGlobalSpringType(GlobalSpringType globalSpringType)
    {
        this.globalSpringType = globalSpringType;
        if (this.globalSpringType == GlobalSpringType.Crazy)
        {
            this.transform.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.None;
            return;
        }
        if (this.globalSpringType == GlobalSpringType.Normal)
        {
            this.transform.GetComponent<Rigidbody>().constraints =
                RigidbodyConstraints.FreezePositionY |
                RigidbodyConstraints.FreezePositionZ |
                RigidbodyConstraints.FreezeRotation;
            return;
        }
        if (this.globalSpringType == GlobalSpringType.Freeze)
        {
            this.transform.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;
            this.transform.GetComponent<Rigidbody>().isKinematic = true;
        }
    }

    public void SetIndexes(int dataBaseOwnerId, int arrayIndex, Person person)
    {
        this.dataBaseOwnerID = dataBaseOwnerId;
        this.arrayIndex = arrayIndex;
        this.person = person;
    }

    public void SetLifeSpan(int birthDate, float age, bool isLiving)
    {
        var myScaleThisPlatformComponent = gameObject.transform.GetChild(ScaleThisChildIndex);
        myScaleThisPlatformComponent.transform.localScale = new Vector3(1.0f, 1.0f, age);
        //myPlatformComponent.transform.localPosition = new Vector3(0, 0, age / 2f);
        lifeSpan = age;
        this.birthDate = birthDate;
        this.endOfPlatformDate = birthDate + (int)age;
        this.isLiving = isLiving;
    }

    public void AddDateQualityInformation((int updated, int original) birthDateQuality, (int updated, int original) deathDateQuality, string dateQualityInformationString)
    {
        this.dateQualityInformationString = dateQualityInformationString;
        this.birthDateQuality = birthDateQuality;
        this.deathDateQuality = deathDateQuality;
    }

    public void addMyBirthQualityBubble()
    {
        var myScaleThisPlatformTransform = gameObject.transform.GetChild(ScaleThisChildIndex);

        var birthConnection = Instantiate(this.bubblePrefabObject, Vector3.zero, Quaternion.identity);  //GameObject.CreatePrimitive(PrimitiveType.Sphere);
        var colorToSet = personDateQualityColors[birthDateQuality.original == birthDateQuality.updated ? 0 : 1];
        birthConnection.GetComponentInChildren<Renderer>().material.SetColor("_Color", colorToSet);

        birthConnection.transform.localScale = Vector3.one * 1.5f;
        birthConnection.transform.parent = myScaleThisPlatformTransform;
        birthConnection.transform.localPosition = new Vector3(0, 0, 0);
    }

    public void SetPersonGender(PersonGenderType personGender)
    {
        this.personGender = personGender;
        //  var renderer = gameObject.GetComponentsInChildren<Renderer>().Where(r => r.CompareTag("GenderColor")).ToArray()[0];
        var renderer = gameObject.GetComponentInChildren<Renderer>();
        renderer.material.SetColor("_Color", personGenderPlatformColors[(int)personGender]);
    }

    public void AddBirthEdge(PersonNode childPersonNode, float myAgeConnectionPointPercent = 0f,
        ChildRelationshipType childRelationshipType = ChildRelationshipType.Biological, int birthDate = 0)
    {
        var foo = new Color(0.3f, 0.4f, 0.6f);
        var childAgeConnectionPointPercent = 0f;
        var parentPlatformTransform = gameObject.transform;
        var parentRidgidbodyComponent = parentPlatformTransform.transform.GetComponent<Rigidbody>();
        var childPlatformTransform = childPersonNode.transform;
        var childRidgidbodyComponent = childPlatformTransform.transform.GetComponent<Rigidbody>();
        if (globalSpringType != GlobalSpringType.Freeze)
        {
            SpringJoint sj = parentRidgidbodyComponent.gameObject.AddComponent<SpringJoint>();
            sj.autoConfigureConnectedAnchor = false;
            sj.anchor = new Vector3(0, 0.5f, myAgeConnectionPointPercent);
            sj.connectedAnchor = new Vector3(0, 0.5f, childAgeConnectionPointPercent);
            sj.enableCollision = true;
            sj.connectedBody = childRidgidbodyComponent;
            sj.spring = 0.01f; // 10.0f;
                               //sj.minDistance = 10.0f;
                               //sj.maxDistance = 500.0f;
        }
        parentBirthConnectionPoint = 
            Instantiate(this.parentPlatformBirthBubble, Vector3.zero, Quaternion.identity);
        //TODO Twins born at the same time are not handled well if one is a boy and the other a girl
        var renderer = parentBirthConnectionPoint.GetComponentInChildren<Renderer>(); //.Where(r => r.CompareTag("GenderColor")).ToArray()[0];
        renderer.material.SetColor("_Color",
            personGenderCapsuleBubbleColors[(int)childPersonNode.personGender]);

        //parentBirthConnectionPoint.transform.localScale = new Vector3(2.0f, 2.0f, 2.0f);
        //leftConnection.transform.localScale = Vector3.one * 2f;
        parentBirthConnectionPoint.transform.parent = parentPlatformTransform.GetChild(0);  // Point to the ScaleThis Section
        parentBirthConnectionPoint.transform.localPosition = new Vector3(0, 0.5f, myAgeConnectionPointPercent);  // 0.5 top of platform

        var triggerTeleportToChildScript = parentBirthConnectionPoint.transform.GetChild(0).GetComponent<TriggerTeleportToChild>();
        triggerTeleportToChildScript.teleportTargetChild = childPlatformTransform;
        triggerTeleportToChildScript.teleportOffset = new Vector3(0, 0.5f, 0);

        childBirthConnectionPoint = //GameObject.CreatePrimitive(PrimitiveType.Sphere);
            Instantiate(this.bubblePrefabObject, Vector3.zero, Quaternion.identity);
        renderer = childBirthConnectionPoint.GetComponentInChildren<Renderer>(); //.Where(r => r.CompareTag("GenderColor")).ToArray()[0];
        renderer.material.SetColor("_Color", clearWhite);
        childBirthConnectionPoint.transform.localScale = Vector3.one * 2f;
        childBirthConnectionPoint.transform.parent = childPlatformTransform.GetChild(0); // Point to the ScaleThis Section
        childBirthConnectionPoint.transform.localPosition = new Vector3(0, 0, childAgeConnectionPointPercent);

        // Mom and Dad Return transportes
        if (this.personGender == PersonGenderType.Male)
        {
            returnToFatherBirthConnectionPoint =
                Instantiate(this.childPlatformReturnToParent, Vector3.zero, Quaternion.identity);
            var returnToFatherRenderer = returnToFatherBirthConnectionPoint.GetComponentInChildren<Renderer>(); 
            returnToFatherRenderer.material.SetColor("_Color", blue);
            var triggerTeleportToFatherScript = returnToFatherBirthConnectionPoint.transform.GetChild(0).GetComponent<TriggerTeleportToChild>();
            triggerTeleportToFatherScript.teleportTargetChild = parentPlatformTransform;
            triggerTeleportToFatherScript.teleportOffset = new Vector3(-3f, .5f, myAgeConnectionPointPercent * this.lifeSpan);
            //returnToFatherBirthConnectionPoint.transform.localScale = Vector3.one * 2f;
            returnToFatherBirthConnectionPoint.transform.parent = childPlatformTransform.GetChild(0); // Point to the ScaleThis Section
            returnToFatherBirthConnectionPoint.transform.localPosition = new Vector3(-3f, 0, 0);
        }
        else if (this.personGender == PersonGenderType.Female)
        {
            returnToMotherBirthConnectionPoint =
                Instantiate(this.childPlatformReturnToParent, Vector3.zero, Quaternion.identity);
            var returnToMotherRenderer = returnToMotherBirthConnectionPoint.GetComponentInChildren<Renderer>();
            returnToMotherRenderer.material.SetColor("_Color", pink);
            var triggerTeleportToMotherScript = returnToMotherBirthConnectionPoint.transform.GetChild(0).GetComponent<TriggerTeleportToChild>();
            triggerTeleportToMotherScript.teleportTargetChild = parentPlatformTransform;
            triggerTeleportToMotherScript.teleportOffset = new Vector3(3f, .5f, myAgeConnectionPointPercent * this.lifeSpan);

            //returnToMotherBirthConnectionPoint.transform.localScale = Vector3.one * 2f;
            returnToMotherBirthConnectionPoint.transform.parent = childPlatformTransform.GetChild(0); // Point to the ScaleThis Section
            returnToMotherBirthConnectionPoint.transform.localPosition = new Vector3(3f, 0, 0);
        }

        GameObject edge = Instantiate(this.birthConnectionPrefabObject, Vector3.zero, Quaternion.identity);
        edge.name = $"Birth {birthDate} {childPersonNode.name}";
        edge.GetComponent<Edge>().CreateEdge(parentBirthConnectionPoint, childBirthConnectionPoint);

        edge.transform.GetChild(ScaleThisChildIndex).GetComponent<Renderer>().material.SetColor("_Color",
            childRelationshipColors[(int)childRelationshipType]);

        edge.transform.parent = transform;
    }

    public void AddMarriageEdge(PersonNode spousePersonNode,
        float myAgeConnectionPointPercent = 0f,
        float spouseAgeConnectionPointPercent = 0f, int marriageEventDate = 0, int marriageLength = 0)
    {
        var myPositionThisPlatformTransform = gameObject.transform;
        var myRidgidbodyComponent = myPositionThisPlatformTransform.GetComponent<Rigidbody>();
        var spousePositionThisPlatformTransform = spousePersonNode.transform;
        var spouseRidgidbodyComponent = spousePositionThisPlatformTransform.GetComponent<Rigidbody>();
        if (globalSpringType != GlobalSpringType.Freeze)
        {
            SpringJoint sj = myRidgidbodyComponent.gameObject.AddComponent<SpringJoint>();
            sj.autoConfigureConnectedAnchor = false;
            sj.anchor = new Vector3(0, 0.5f, myAgeConnectionPointPercent);   // - 0.5f
            sj.connectedAnchor = new Vector3(0, 0.5f, spouseAgeConnectionPointPercent);  // - 0.5f
            sj.enableCollision = true;
            sj.connectedBody = spouseRidgidbodyComponent;
            sj.spring = 5f; // 50.0f;
            sj.minDistance = 20.0f;
            sj.maxDistance = 80.0f;
        }
        parentBirthConnectionPoint = new GameObject(); // GameObject.CreatePrimitive(PrimitiveType.Sphere);
        //leftConnection.GetComponent<Renderer>().material.SetColor("_Color", Color.yellow);

        parentBirthConnectionPoint.transform.localScale = Vector3.one * 2f;
        parentBirthConnectionPoint.transform.parent = myPositionThisPlatformTransform.GetChild(0);  // Point to the ScaleThis Section
        parentBirthConnectionPoint.transform.localPosition = new Vector3(0, 0, myAgeConnectionPointPercent);

        childBirthConnectionPoint = new GameObject(); // GameObject.CreatePrimitive(PrimitiveType.Sphere);
        //rightConnection.GetComponent<Renderer>().material.SetColor("_Color", Color.yellow);
        childBirthConnectionPoint.transform.localScale = Vector3.one * 2f;
        childBirthConnectionPoint.transform.parent = spousePositionThisPlatformTransform.GetChild(0);  // Point to the ScaleThis Section
        childBirthConnectionPoint.transform.localPosition = new Vector3(0, 0, spouseAgeConnectionPointPercent);
        //

        GameObject edge = Instantiate(this.marriageConnectionPrefabObject, Vector3.zero, Quaternion.identity);
        edge.name = $"Marriage {marriageEventDate}, to {spousePersonNode.name}, duration {marriageLength}.";
        edge.GetComponent<Edge>().CreateEdge(parentBirthConnectionPoint, childBirthConnectionPoint);
        edge.GetComponent<Edge>().SetEdgeEventLength(marriageLength, marriageConnectionXScale);
        var material = edge.transform.GetChild(0).GetComponent<Renderer>().material;
        material.SetColor("_Color", new Color(1.0f, 0.92f, 0.01f, 0.6f));
        //material.SetOverrideTag("RenderMode", "Transparent");

        edge.transform.parent = myPositionThisPlatformTransform;
    }

    public void SetDebugAddMotionSetting(bool newDebugAddMotionSetting)
    {
        this.debugAddMotion = newDebugAddMotionSetting;
    }
}