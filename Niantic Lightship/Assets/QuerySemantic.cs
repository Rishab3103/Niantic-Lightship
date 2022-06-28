using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.UI;
using Niantic.ARDK.Utilities.Input.Legacy;
using Niantic.ARDK.AR;
using Niantic.ARDK.Utilities;

using Niantic.ARDK.AR.ARSessionEventArgs;
using Niantic.ARDK.AR.Configuration;

using Niantic.ARDK.AR.Awareness;
using Niantic.ARDK.AR.Awareness.Semantics;

using Niantic.ARDK.Extensions;

public class QuerySemantic : MonoBehaviour
{
    ISemanticBuffer _currentBuffer;

    public ARSemanticSegmentationManager _semanticManager;
    public Camera _camera;

    public Text _text;

    void Start()
    {
        //add a callback for catching the updated semantic buffer
        _semanticManager.SemanticBufferUpdated += OnSemanticsBufferUpdated;
    }

    private void OnSemanticsBufferUpdated(ContextAwarenessStreamUpdatedArgs<ISemanticBuffer> args)
    {
        //get the current buffer
        _currentBuffer = args.Sender.AwarenessBuffer;
    }

    //our list of possible items to find.
    string[] items = {
        "grass",
        "water",
        "foliage"
    };

    //timer for how long the user has to find the current item
    float _findTimer = 0.0f;
    //cooldown timer for how long to wait before issuing another find item request
    float _waitTimer = 2.0f;

    //our score to track for our win condition
    int _score = 0;

    //the current item we are looking for
    string _thingToFind = "";
    //if we found the item this frame.
    bool _found = true;

    //function to pick a randon item to fetch from the list
    void PickRandomItemToFind()
    {
        int randCh = (int)UnityEngine.Random.Range(0.0f, 3.0f);
        _thingToFind = items[randCh];
        _findTimer = UnityEngine.Random.Range(5.0f, 10.0f);
        _found = false;
    }

    // Update is called once per frame
    void Update()
    {
        //our win condition you found 5 things
        if (_score > 5)
        {
            _text.text = "Now that I have all of the parts I can make your quest reward. +2 vorpal shoulderpads!";
            return;
        }

        //tick down our timers
        _findTimer -= Time.deltaTime;
        _waitTimer -= Time.deltaTime;

        //wait here inbetween quests for a bit
        if (_waitTimer > 0.0f)
            return;

        //the alloted time to find an item is expired
        if (_findTimer <= 0.0f)
        {
            //fail condition if we did not find it.
            if (_found == false)
            {
                _text.text = "Quest failed";

                _waitTimer = 2.0f;
                _found = true;
                return;
            }

            //otherwise pick a new thing to find
            PickRandomItemToFind();
            _text.text = "Hey there adventurer could you find me some " + _thingToFind;
        }

        //input functions
        if (PlatformAgnosticInput.touchCount <= 0) { return; }
        var touch = PlatformAgnosticInput.GetTouch(0);
        if (touch.phase == TouchPhase.Began)
        {
            //get the current touch position
            int x = (int)touch.position.x;
            int y = (int)touch.position.y;

            //ask the semantic manager if the item we are looking for is in the selected pixel
            if (_semanticManager.SemanticBufferProcessor.DoesChannelExistAt(x, y, _thingToFind))
            {
                //if so then trigger success for this request and reset timers
                _text.text = "Thanks adventurer, this is just what i was looking for!";
                _findTimer = 0.0f;
                _waitTimer = 2.0f;
                _score++;
                _found = true;
            }
            else
            {
                //if not look at what is in that pixel and give the user some feedback
                string[] channelsNamesInPixel = _semanticManager.SemanticBufferProcessor.GetChannelNamesAt(x, y);
                string found;
                if (channelsNamesInPixel.Length > 0)
                    found = channelsNamesInPixel[0];
                else
                    found = "thin air";

                _text.text = "Nah thats " + found + ", i was after " + _thingToFind;
            }
        }
    }
}