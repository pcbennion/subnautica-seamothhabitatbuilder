using System;
using System.Text;
using System.Collections.Generic;
using UnityEngine;

namespace SeamothHabitatBuilder.MonoBehaviors
{
    //=========================================================================
    // SeamothBuilder
    //
    // Copies most of the functionality of the BuilderTool class, except for 
    // battery power, the on-tool display, and any mesh animations from the
    // handheld tool
    //
    // Effects have been tweaked to come out from below the cockpit, and the
    // tool has slightly greater range to accommodate the size of the Seamoth
    //  
    // Annotations added to most code for clarity
    //  
    // BuilderTool source code was decompiled using dnSpy
    //=========================================================================

    class SeamothBuilder : MonoBehaviour
    {
        // New variables
        private Vehicle vehicle;
        public bool enable { get; set; }

        // VFX and sounds
        public Transform nozzleLeft;
        public Transform nozzleRight;
        public Transform beamLeft;
        public Transform beamRight;
        public float nozzleRotationSpeed = 10f;
        [Range(0.01f, 5f)]
        public float pointSwitchTimeMin = 0.1f;
        [Range(0.01f, 5f)]
        public float pointSwitchTimeMax = 1f;
        public Animator animator;
        public FMOD_CustomLoopingEmitter buildSound;
        public FMODAsset completeSound;

        // Misc internal variables
        private const float hitRange = 40f;
        private bool isConstructing;
        private Constructable constructable;
        private int handleInputFrame = -1;
        private Vector3 leftPoint = Vector3.zero;
        private Vector3 rightPoint = Vector3.zero;
        private float leftConstructionTime;
        private float rightConstructionTime;
        private float leftConstructionInterval;
        private float rightConstructionInterval;
        private Vector3 leftConstructionPoint;
        private Vector3 rightConstructionPoint;
        private string deconstructText;
        private string constructText;

        //=====================================================================
        // Start
        //
        // Initialize the object gracefully
        //=====================================================================
        private void Start() 
        {
            // Get a reference to the parent vehicle and initialize "enable"
            this.vehicle = GetComponent<Vehicle>();
            this.enable = false;

            Console.WriteLine(string.Format("[SeamothHabitatBuilder] Techtype: {0}", MainPatcher.SeamothBuilderModule.AsString()));

            // Reset all beam effects and the tooltip text
            this.SetBeamActive(false);
            this.UpdateText();
        }

        //=====================================================================
        // OnDisable
        //
        // Euthanize the object gracefully
        //=====================================================================
        private void OnDisable()
        {
            // Make sure to end any sounds
            this.buildSound.Stop();
        }

        //=====================================================================
        // Update
        //
        // Do frame-to-frame processing
        //=====================================================================
        private void Update()
        {
            // Do nothing if the Builder is not selected or if the player is in menus
            if (!enable || Player.main.GetPDA().isOpen) return;

            // Disable if the parent vehicle is not being operated (or if the module has been removed)
            if (vehicle.modules.GetCount(MainPatcher.SeamothBuilderModule) <= 0 || !vehicle.GetPilotingMode())
            {
                enable = false;
                return;
            }

            Console.WriteLine(string.Format("[SeamothHabitatBuilder] Active Techtype: {0}", vehicle.GetSlotBinding(vehicle.GetActiveSlotID()).AsString()));
            this.HandleInput();
        }

        //=====================================================================
        // LateUpdate
        //
        // Handle low priority processing like vfx and animation
        //=====================================================================
        private void LateUpdate() 
        {
            Quaternion b = Quaternion.identity;
            Quaternion b2 = Quaternion.identity;
            bool flag = this.constructable != null;
            if (this.isConstructing != flag)
            {
                this.isConstructing = flag;
                if (this.isConstructing)
                {
                    this.leftConstructionInterval = UnityEngine.Random.Range(this.pointSwitchTimeMin, this.pointSwitchTimeMax);
                    this.rightConstructionInterval = UnityEngine.Random.Range(this.pointSwitchTimeMin, this.pointSwitchTimeMax);
                    this.leftConstructionPoint = this.constructable.GetRandomConstructionPoint();
                    this.rightConstructionPoint = this.constructable.GetRandomConstructionPoint();
                }
                else
                {
                    this.leftConstructionTime = 0f;
                    this.rightConstructionTime = 0f;
                }
            }
            else if (this.isConstructing)
            {
                this.leftConstructionTime += Time.deltaTime;
                this.rightConstructionTime += Time.deltaTime;
                if (this.leftConstructionTime >= this.leftConstructionInterval)
                {
                    this.leftConstructionTime %= this.leftConstructionInterval;
                    this.leftConstructionInterval = UnityEngine.Random.Range(this.pointSwitchTimeMin, this.pointSwitchTimeMax);
                    this.leftConstructionPoint = this.constructable.GetRandomConstructionPoint();
                }
                if (this.rightConstructionTime >= this.rightConstructionInterval)
                {
                    this.rightConstructionTime %= this.rightConstructionInterval;
                    this.rightConstructionInterval = UnityEngine.Random.Range(this.pointSwitchTimeMin, this.pointSwitchTimeMax);
                    this.rightConstructionPoint = this.constructable.GetRandomConstructionPoint();
                }
                this.leftPoint = this.nozzleLeft.parent.InverseTransformPoint(this.leftConstructionPoint);
                this.rightPoint = this.nozzleRight.parent.InverseTransformPoint(this.rightConstructionPoint);
                Debug.DrawLine(this.nozzleLeft.position, this.leftConstructionPoint, Color.white);
                Debug.DrawLine(this.nozzleRight.position, this.rightConstructionPoint, Color.white);
            }
            if (this.isConstructing)
            {
                b = Quaternion.LookRotation(this.leftPoint, Vector3.up);
                b2 = Quaternion.LookRotation(this.rightPoint, Vector3.up);
                Vector3 localScale = this.beamLeft.localScale;
                localScale.z = this.leftPoint.magnitude;
                this.beamLeft.localScale = localScale;
                localScale = this.beamRight.localScale;
                localScale.z = this.rightPoint.magnitude;
                this.beamRight.localScale = localScale;
                Debug.DrawLine(this.nozzleLeft.position, this.leftConstructionPoint, Color.white);
                Debug.DrawLine(this.nozzleRight.position, this.rightConstructionPoint, Color.white);
            }
            float t = this.nozzleRotationSpeed * Time.deltaTime;
            this.nozzleLeft.localRotation = Quaternion.Slerp(this.nozzleLeft.localRotation, b, t);
            this.nozzleRight.localRotation = Quaternion.Slerp(this.nozzleRight.localRotation, b2, t);
            this.SetBeamActive(this.isConstructing);
            if (this.isConstructing)
            {
                this.buildSound.Play();
            }
            else
            {
                this.buildSound.Stop();
            }
            this.constructable = null;
        }

        //=====================================================================
        // HandleInput
        //
        // Called by Update. Checks for input events and operates the Builder
        // accordingly
        //=====================================================================
        private void HandleInput() 
        {
            if (this.handleInputFrame == Time.frameCount)
            {
                return;
            }
            this.handleInputFrame = Time.frameCount;
            if (Builder.isPlacing || !AvatarInputHandler.main.IsEnabled())
            {
                return;
            }
            Targeting.AddToIgnoreList(Player.main.gameObject);
            GameObject gameObject;
            float num;
            // Range increased to 40 to give the seamoth more room
            Targeting.GetTarget(hitRange, out gameObject, out num, null);
            if (gameObject == null)
            {
                return;
            }
            // Bring up the construct menu on alt tool use
            //      (because the seamoth has lights bound to right hand)
            if (GameInput.GetButtonDown(GameInput.Button.AltTool)) {
                uGUI_BuilderMenu.Show();
                return;
            }
            bool constructHeld = GameInput.GetButtonHeld(GameInput.Button.LeftHand);
            bool deconstructDown = GameInput.GetButtonDown(GameInput.Button.Deconstruct);
            bool deconstructHeld = GameInput.GetButtonHeld(GameInput.Button.Deconstruct);
            Constructable constructable = gameObject.GetComponentInParent<Constructable>();
            if (constructable != null && num > constructable.placeMaxDistance)
            {
                constructable = null;
            }
            if (constructable != null)
            {
                this.OnHover(constructable);
                string text;
                if (constructHeld)
                {
                    this.Construct(constructable, true);
                }
                else if (constructable.DeconstructionAllowed(out text))
                {
                    if (deconstructHeld)
                    {
                        if (constructable.constructed)
                        {
                            constructable.SetState(false, false);
                        }
                        else
                        {
                            this.Construct(constructable, false);
                        }
                    }
                }
                else if (deconstructDown && !string.IsNullOrEmpty(text))
                {
                    ErrorMessage.AddMessage(text);
                }
            }
            else
            {
                BaseDeconstructable baseDeconstructable = gameObject.GetComponentInParent<BaseDeconstructable>();
                if (baseDeconstructable == null)
                {
                    BaseExplicitFace componentInParent = gameObject.GetComponentInParent<BaseExplicitFace>();
                    if (componentInParent != null)
                    {
                        baseDeconstructable = componentInParent.parent;
                    }
                }
                if (baseDeconstructable != null)
                {
                    string text;
                    if (baseDeconstructable.DeconstructionAllowed(out text))
                    {
                        this.OnHover(baseDeconstructable);
                        if (deconstructDown)
                        {
                            baseDeconstructable.Deconstruct();
                        }
                    }
                    else if (deconstructDown && !string.IsNullOrEmpty(text))
                    {
                        ErrorMessage.AddMessage(text);
                    }
                }
            }
        }

        //=====================================================================
        // UpdateText
        //
        // Sets tooltip text
        //=====================================================================
        private void UpdateText()
        {
            string buttonFormat = LanguageCache.GetButtonFormat("ConstructFormat", GameInput.Button.LeftHand);
            string buttonFormat2 = LanguageCache.GetButtonFormat("DeconstructFormat", GameInput.Button.Deconstruct);
            this.constructText = Language.main.GetFormat<string, string>("ConstructDeconstructFormat", buttonFormat, buttonFormat2);
            this.deconstructText = buttonFormat2;
        }

        //=====================================================================
        // Construct
        //
        // Figure out whether we're starting, ending, or continuing
        // construction and plays the appropriate sounds
        //=====================================================================
        private bool Construct(Constructable c, bool state) 
        {
            if (c != null && !c.constructed)
            {
                bool constructed = c.constructed;
                bool flag = (!state) ? c.Deconstruct() : c.Construct();
                if (flag)
                {
                    this.constructable = c;
                }
                else if (state && !constructed)
                {
                    global::Utils.PlayFMODAsset(this.completeSound, c.transform, 20f);
                }
                return true;
            }
            return false;
        }

        //=====================================================================
        // OnHover (Constructable)
        //
        // Displays tooltip text when the player is looking at a constructable
        // object
        //=====================================================================
        private void OnHover(Constructable constructable)
        {
            HandReticle main = HandReticle.main;
            if (constructable.constructed)
            {
                main.SetInteractText(Language.main.Get(constructable.techType), this.deconstructText, false, false, false);
            }
            else
            {
                StringBuilder stringBuilder = new StringBuilder();
                stringBuilder.AppendLine(this.constructText);
                foreach (KeyValuePair<TechType, int> keyValuePair in constructable.GetRemainingResources())
                {
                    TechType key = keyValuePair.Key;
                    string text = Language.main.Get(key);
                    int value = keyValuePair.Value;
                    if (value > 1)
                    {
                        stringBuilder.AppendLine(Language.main.GetFormat<string, int>("RequireMultipleFormat", text, value));
                    }
                    else
                    {
                        stringBuilder.AppendLine(text);
                    }
                }
                main.SetInteractText(Language.main.Get(constructable.techType), stringBuilder.ToString(), false, false, false);
                main.SetProgress(constructable.amount);
                main.SetIcon(HandReticle.IconType.Progress, 1.5f);
            }
        }

        //=====================================================================
        // OnHover (Deconstructable)
        //
        // As above, but only for DEconstructable objects
        //=====================================================================
        private void OnHover(BaseDeconstructable deconstructable)
        {
            HandReticle main = HandReticle.main;
            main.SetInteractInfo(deconstructable.Name, this.deconstructText);
        }

        //=====================================================================
        // SetBeamActive
        //
        // Acts as a toggle for the beam effects
        //=====================================================================
        private void SetBeamActive(bool state) 
        {
            if (this.beamLeft != null)
            {
                this.beamLeft.gameObject.SetActive(state);
            }
            if (this.beamRight != null)
            {
                this.beamRight.gameObject.SetActive(state);
            }
        }
    }
}