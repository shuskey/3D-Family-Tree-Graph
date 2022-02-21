using Assets.Scripts.DataObjects;
using Assets.Scripts.Enums;
using Newtonsoft.Json;
using StarterAssets;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class EventDetailsHandler : MonoBehaviour
{
    public Sprite noImageForThisEvent;    
    public GameObject imageGameObject;
    public GameObject yearAndTitleGameObject;
    public GameObject descriptionGameObject;
    public GameObject dateRangeGameObject;
    public GameObject panelCountGameObject;
    public GameObject additionalInstructionsGameObject;

    private TopEvent topEventObject;

    // Start is called before the first frame update
    void Start()
    {

    }

    public void ClearEventDisplay()
    {
        // set text items to ""
        // make the panel not visible

        topEventObject = null;
        yearAndTitleGameObject.GetComponent<Text>().text = null;
        descriptionGameObject.GetComponent<Text>().text = null;
        dateRangeGameObject.GetComponent<Text>().text = null;
        panelCountGameObject.GetComponent<Text>().text = null;
        additionalInstructionsGameObject.GetComponent<Text>().text = null;

        imageGameObject.GetComponent<Image>().sprite = noImageForThisEvent;
    }

    public void DisplayThisEvent(TopEvent eventToDisplay, int currentEventIndex, int numberOfEvents, Texture2D eventImage_Texture)
    {
        //make the panel visible
        topEventObject = eventToDisplay;

        yearAndTitleGameObject.GetComponent<Text>().text =  topEventObject.year + " " + topEventObject.itemLabel;
        descriptionGameObject.GetComponent<Text>().text =  topEventObject.description;
        string dateRangeString = "";
        if (!String.IsNullOrEmpty(topEventObject.eventStartDate)) 
            dateRangeString = JsonConvert.DeserializeObject<DateTime>(("\"" + topEventObject.eventStartDate + "\"")).ToString("dd MMM yyyy");
        if (!String.IsNullOrEmpty(topEventObject.eventEndDate))
            dateRangeString = dateRangeString + " - " + JsonConvert.DeserializeObject<DateTime>(("\"" + topEventObject.eventEndDate + "\"")).ToString("dd MMM yyyy");
        dateRangeGameObject.GetComponent<Text>().text =  dateRangeString;
        var eventTally = numberOfEvents == 0 ? "0 / 0" : $"{currentEventIndex + 1} / {numberOfEvents}";
        panelCountGameObject.GetComponent<Text>().text =  $"Event: {eventTally}";

        additionalInstructionsGameObject.GetComponent<Text>().text = "< or >";

        var cropSize = Math.Min(eventImage_Texture.width, eventImage_Texture.height);
        var xStart = (eventImage_Texture.width - cropSize) / 2;
        var yStart = (eventImage_Texture.height - cropSize) / 2;

        imageGameObject.GetComponent<Image>().sprite = Sprite.Create(eventImage_Texture, new Rect(xStart, yStart, cropSize, cropSize), new Vector2(0.5f, 0.5f), 100f);
    }

    // Update is called once per frame
    void Update()
    {

    }
}
