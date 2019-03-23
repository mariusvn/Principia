﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace principia {
namespace ksp_plugin_adapter {

internal class MainWindow : SupervisedWindowRenderer {
  // Update this section before each release.
  private const String next_release_name_ = "Fano";
  private const int next_release_lunation_number_ = 238;
  private DateTimeOffset next_release_date_ =
      new DateTimeOffset(2019, 04, 05, 08, 51, 00, TimeSpan.Zero);

  public MainWindow(SupervisedWindowRenderer.ISupervisor supervisor,
                    FlightPlanner flight_planner,
                    ReferenceFrameSelector plotting_frame_selector)
      : base(supervisor) {
    flight_planner_ = flight_planner;
    plotting_frame_selector_ = plotting_frame_selector;
  }

  public void Initialize(IntPtr plugin) {
    plugin_ = plugin;
  }

  public void SelectTargetCelestial(MapObject map_object) {
    if (selecting_target_celestial_) {
      FlightGlobals.fetch.SetVesselTarget(map_object.celestialBody);
      selecting_target_celestial_ = false;
    } else if (PlanetariumCamera.fetch.target != map_object) {
      PlanetariumCamera.fetch.SetTarget(map_object);
    }
  }

  public void SelectActiveVesselTarget(MapObject map_object,
                                       bool set_planetarium_camera) {
    if (selecting_active_vessel_target) {
      FlightGlobals.fetch.SetVesselTarget(map_object.vessel);
      selecting_active_vessel_target = false;
    } else if (set_planetarium_camera &&
               PlanetariumCamera.fetch.target != map_object) {
      PlanetariumCamera.fetch.SetTarget(map_object);
    }
  }

  public bool selecting_active_vessel_target { get; private set; } = false;

  public bool display_patched_conics { get; private set; } = false;

  public double history_length {
    get {
      return history_lengths_[history_length_index_];
    }
  }

  public double prediction_length_tolerance {
    get {
      return prediction_length_tolerances_[prediction_length_tolerance_index_];
    }
  }

  public Int64 prediction_steps {
    get {
      return prediction_steps_[prediction_steps_index_];
    }
  }

  public new void Load(ConfigNode node) {
    base.Load(node);

    String show_ksp_features_value =
        node.GetAtMostOneValue("show_ksp_features");
    if (show_ksp_features_value != null) {
      show_ksp_features_ = Convert.ToBoolean(show_ksp_features_value);
    }
    String show_logging_settings_value =
        node.GetAtMostOneValue("show_logging_settings");
    if (show_logging_settings_value != null) {
      show_logging_settings_ = Convert.ToBoolean(show_logging_settings_value);
    }
    String show_prediction_settings_value =
        node.GetAtMostOneValue("show_prediction_settings");
    if (show_prediction_settings_value != null) {
      show_prediction_settings_ =
          Convert.ToBoolean(show_prediction_settings_value);
    }

    String prediction_length_tolerance_index_value =
        node.GetAtMostOneValue("prediction_length_tolerance_index");
    if (prediction_length_tolerance_index_value != null) {
      prediction_length_tolerance_index_ =
          Convert.ToInt32(prediction_length_tolerance_index_value);
    }
    String prediction_steps_index_value =
        node.GetAtMostOneValue("prediction_steps_index");
    if (prediction_steps_index_value != null) {
      prediction_steps_index_ = Convert.ToInt32(prediction_steps_index_value);
    }
    String history_length_index_value =
        node.GetAtMostOneValue("history_length_index");
    if (history_length_index_value != null) {
      history_length_index_ = Convert.ToInt32(history_length_index_value);
    }

    String buffered_logging_value =
        node.GetAtMostOneValue("buffered_logging");
    if (buffered_logging_value != null) {
      buffered_logging_ = Convert.ToInt32(buffered_logging_value);
    }
    String stderr_logging_value = node.GetAtMostOneValue("stderr_logging");
    if (stderr_logging_value != null) {
      stderr_logging_ = Convert.ToInt32(stderr_logging_value);
    }
    String suppressed_logging_value =
        node.GetAtMostOneValue("suppressed_logging");
    if (suppressed_logging_value != null) {
      suppressed_logging_ = Convert.ToInt32(suppressed_logging_value);
    }
    String verbose_logging_value = node.GetAtMostOneValue("verbose_logging");
    if (verbose_logging_value != null) {
      verbose_logging_ = Convert.ToInt32(verbose_logging_value);
    }

    String must_record_journal_value =
        node.GetAtMostOneValue("must_record_journal");
    if (must_record_journal_value != null) {
      must_record_journal_ = Convert.ToBoolean(must_record_journal_value);
    }

    Log.SetBufferedLogging(buffered_logging_);
    Log.SetSuppressedLogging(suppressed_logging_);
    Log.SetStderrLogging(stderr_logging_);
    Log.SetVerboseLogging(verbose_logging_);

    if (must_record_journal_) {
      journaling_ = true;
      Log.ActivateRecorder(true);
    }
  }

  public new void Save(ConfigNode node) {
    base.Save(node);

    node.SetValue("show_ksp_features",
                  show_ksp_features_,
                  createIfNotFound : true);
    node.SetValue("show_logging_settings",
                  show_logging_settings_,
                  createIfNotFound : true);
    node.SetValue("show_prediction_settings",
                  show_prediction_settings_,
                  createIfNotFound : true);

    node.SetValue("prediction_length_tolerance_index",
                  prediction_length_tolerance_index_,
                  createIfNotFound : true);
    node.SetValue("prediction_steps_index",
                  prediction_steps_index_,
                  createIfNotFound : true);
    node.SetValue("history_length_index",
                  history_length_index_,
                  createIfNotFound : true);

    node.SetValue("buffered_logging",
                  buffered_logging_,
                  createIfNotFound : true);
    node.SetValue("stderr_logging",
                  stderr_logging_,
                  createIfNotFound : true);
    node.SetValue("suppressed_logging",
                  suppressed_logging_,
                  createIfNotFound : true);
    node.SetValue("verbose_logging",
                  verbose_logging_,
                  createIfNotFound : true);

    node.SetValue("must_record_journal",
                  must_record_journal_,
                  createIfNotFound : true);
  }

  protected override string Title { get; } = "Principia";

  protected override void RenderWindow(int window_id) {
    using (new UnityEngine.GUILayout.VerticalScope()) {
      if (plugin_ == IntPtr.Zero) {
        UnityEngine.GUILayout.TextArea(text : "Plugin is not started");
      }
      if (DateTimeOffset.Now > next_release_date_) {
        UnityEngine.GUILayout.TextArea(
            "Announcement: the new moon of lunation number " +
            next_release_lunation_number_ +
            " has come; please download the latest Principia release, " +
            next_release_name_ + ".");
      }
      String version;
      String unused_build_date;
      Interface.GetVersion(build_date: out unused_build_date,
                           version: out version);
      UnityEngine.GUILayout.TextArea(version);
      bool changed_history_length = false;
      RenderSelector(history_lengths_,
                     ref history_length_index_,
                     "Max history length",
                     ref changed_history_length,
                     "{0:0.00e00} s");
      if (MapView.MapIsEnabled &&
          FlightGlobals.ActiveVessel?.orbitTargeter != null) {
        using (new UnityEngine.GUILayout.HorizontalScope()) {
          selecting_active_vessel_target = UnityEngine.GUILayout.Toggle(
              selecting_active_vessel_target, "Select target vessel...");
          if (selecting_active_vessel_target) {
            selecting_target_celestial_ = false;
          }
          if (FlightGlobals.fetch.VesselTarget?.GetVessel()) {
            UnityEngine.GUILayout.Label(
                "Target: " +
                    FlightGlobals.fetch.VesselTarget.GetVessel().vesselName,
                UnityEngine.GUILayout.ExpandWidth(true));
            if (UnityEngine.GUILayout.Button("Clear",
                                              UnityEngine.GUILayout.Width(50))) {
              selecting_active_vessel_target = false;
              FlightGlobals.fetch.SetVesselTarget(null);
            }
            if (UnityEngine.GUILayout.Button("Switch To")) {
              var focus_object =
                  new KSP.UI.Screens.Mapview.MapContextMenuOptions.FocusObject(
                      FlightGlobals.fetch.VesselTarget.GetVessel().orbitDriver);
              focus_object.onOptionSelected();
            }
          }
        }
      } else {
        selecting_active_vessel_target = false;
      }
      if (plugin_ != IntPtr.Zero) {
        plotting_frame_selector_.RenderButton();
        flight_planner_.RenderButton();
      }
      ToggleableSection(name   : "Prediction Settings",
                        show   : ref show_prediction_settings_,
                        render : RenderPredictionSettings);
      ToggleableSection(name   : "KSP features",
                        show   : ref show_ksp_features_,
                        render : RenderKSPFeatures);
      ToggleableSection(name   : "Logging Settings",
                        show   : ref show_logging_settings_,
                        render : RenderLoggingSettings);
  #if CRASH_BUTTON
      ToggleableSection(name   : "CRASH",
                        show   : ref show_crash_options_,
                        render : CrashOptions);
  #endif
    }
    UnityEngine.GUI.DragWindow();
  }

  private delegate void GUIRenderer();

  private void RenderKSPFeatures() {
    display_patched_conics = UnityEngine.GUILayout.Toggle(
        value : display_patched_conics,
        text  : "Display patched conics (do not use for flight planning!)");
    Sun.Instance.sunFlare.enabled =
        UnityEngine.GUILayout.Toggle(value : Sun.Instance.sunFlare.enabled,
                                     text  : "Enable Sun lens flare");
    if (MapView.MapIsEnabled &&
        FlightGlobals.ActiveVessel?.orbitTargeter != null) {
      using (new UnityEngine.GUILayout.HorizontalScope()) {
        selecting_target_celestial_ = UnityEngine.GUILayout.Toggle(
            selecting_target_celestial_, "Select target celestial...");
        if (selecting_target_celestial_) {
          selecting_active_vessel_target = false;
        }
        CelestialBody target_celestial =
            FlightGlobals.fetch.VesselTarget as CelestialBody;
        if (target_celestial) {
          UnityEngine.GUILayout.Label("Target: " + target_celestial.name,
                                      UnityEngine.GUILayout.ExpandWidth(true));
          if (UnityEngine.GUILayout.Button("Clear",
                                           UnityEngine.GUILayout.Width(50))) {
            selecting_target_celestial_ = false;
            FlightGlobals.fetch.SetVesselTarget(null);
          }
        }
      }
    } else {
      selecting_target_celestial_ = false;
    }
  }

  private void RenderLoggingSettings() {
#if SELECTABLE_PLOT_METHOD
    using (new UnityEngine.GUILayout.HorizontalScope()) {
      UnityEngine.GUILayout.Label("Чебышёв plotting method:");
      for (int i = 0; i < чебышёв_plotting_methods_count; ++i) {
        if (UnityEngine.GUILayout.Toggle(чебышёв_plotting_method_ == i,
                                         i.ToString())) {
          чебышёв_plotting_method_ = i;
        }
      }
    }
#endif
    using (new UnityEngine.GUILayout.HorizontalScope()) {
      UnityEngine.GUILayout.Label(text : "Verbose level:");
      if (UnityEngine.GUILayout.Button(
              text    : "←",
              options : UnityEngine.GUILayout.Width(50))) {
        Log.SetVerboseLogging(Math.Max(verbose_logging_ - 1, 0));
        verbose_logging_ = Log.GetVerboseLogging();
      }
      UnityEngine.GUILayout.TextArea(
          text    : Log.GetVerboseLogging().ToString(),
          options : UnityEngine.GUILayout.Width(50));
      if (UnityEngine.GUILayout.Button(
              text    : "→",
              options : UnityEngine.GUILayout.Width(50))) {
        Log.SetVerboseLogging(Math.Min(verbose_logging_ + 1, 4));
        verbose_logging_ = Log.GetVerboseLogging();
      }
    }
    int column_width = 75;
    using (new UnityEngine.GUILayout.HorizontalScope()) {
      UnityEngine.GUILayout.Space(column_width);
      UnityEngine.GUILayout.Label(
          text    : "Log",
          options : UnityEngine.GUILayout.Width(column_width));
      UnityEngine.GUILayout.Label(
          text    : "stderr",
          options : UnityEngine.GUILayout.Width(column_width));
      UnityEngine.GUILayout.Label(
          text    : "Flush",
          options : UnityEngine.GUILayout.Width(column_width));
    }
    using (new UnityEngine.GUILayout.HorizontalScope()) {
      UnityEngine.GUILayout.Space(column_width);
      if (UnityEngine.GUILayout.Button(
              text    : "↑",
              options : UnityEngine.GUILayout.Width(column_width))) {
        Log.SetSuppressedLogging(Math.Max(suppressed_logging_ - 1, 0));
        suppressed_logging_ = Log.GetSuppressedLogging();
      }
      if (UnityEngine.GUILayout.Button(
              text    : "↑",
              options : UnityEngine.GUILayout.Width(column_width))) {
        Log.SetStderrLogging(Math.Max(stderr_logging_ - 1, 0));
        stderr_logging_ = Log.GetStderrLogging();
      }
      if (UnityEngine.GUILayout.Button(
              text    : "↑",
              options : UnityEngine.GUILayout.Width(column_width))) {
        Log.SetBufferedLogging(Math.Max(buffered_logging_ - 1, -1));
        buffered_logging_ = Log.GetBufferedLogging();
      }
    }
    for (int severity = 0; severity <= 3; ++severity) {
      using (new UnityEngine.GUILayout.HorizontalScope()) {
        UnityEngine.GUILayout.Label(
            text    : Log.severity_names[severity],
            options : UnityEngine.GUILayout.Width(column_width));
        UnityEngine.GUILayout.Toggle(
            value   : severity >= Log.GetSuppressedLogging(),
            text    : "",
            options : UnityEngine.GUILayout.Width(column_width));
        UnityEngine.GUILayout.Toggle(
            value   : severity >= Log.GetStderrLogging(),
            text    : "",
            options : UnityEngine.GUILayout.Width(column_width));
        UnityEngine.GUILayout.Toggle(
            value   : severity > Log.GetBufferedLogging(),
            text    : "",
            options : UnityEngine.GUILayout.Width(column_width));
      }
    }
    using (new UnityEngine.GUILayout.HorizontalScope()) {
      UnityEngine.GUILayout.Space(column_width);
      if (UnityEngine.GUILayout.Button(
              text    : "↓",
              options : UnityEngine.GUILayout.Width(column_width))) {
        Log.SetSuppressedLogging(Math.Min(suppressed_logging_ + 1, 3));
        suppressed_logging_ = Log.GetSuppressedLogging();
      }
      if (UnityEngine.GUILayout.Button(
              text    : "↓",
              options : UnityEngine.GUILayout.Width(column_width))) {
        Log.SetStderrLogging(Math.Min(stderr_logging_ + 1, 3));
        stderr_logging_ = Log.GetStderrLogging();
      }
      if (UnityEngine.GUILayout.Button(
              text    : "↓",
              options : UnityEngine.GUILayout.Width(column_width))) {
        Log.SetBufferedLogging(Math.Min(buffered_logging_ + 1, 3));
        buffered_logging_ = Log.GetBufferedLogging();
      }
    }
    UnityEngine.GUILayout.TextArea("Journalling is " +
                                   (journaling_ ? "ON" : "OFF"));
    must_record_journal_ = UnityEngine.GUILayout.Toggle(
        value   : must_record_journal_,
        text    : "Record journal (starts on load)");
    if (journaling_ && !must_record_journal_) {
      // We can deactivate a recorder at any time, but in order for replaying to
      // work, we should only activate one before creating a plugin.
      journaling_ = false;
      Interface.ActivateRecorder(false);
    }
  }

  private void RenderPredictionSettings() {
    bool changed_settings = false;
    RenderSelector(prediction_length_tolerances_,
                   ref prediction_length_tolerance_index_,
                   "Tolerance",
                   ref changed_settings,
                   "{0:0.0e0} m");
    RenderSelector(prediction_steps_,
                   ref prediction_steps_index_,
                   "Steps",
                   ref changed_settings,
                   "{0:0.00e0}");
  }

  private static void RenderSelector<T>(T[] array,
                                        ref int index,
                                        String label,
                                        ref bool changed,
                                        String format) {
    using (new UnityEngine.GUILayout.HorizontalScope()) {
      UnityEngine.GUILayout.Label(text    : label + ":",
                                  options : UnityEngine.GUILayout.Width(150));
      if (UnityEngine.GUILayout.Button(
              text    : index == 0 ? "min" : "-",
              options : UnityEngine.GUILayout.Width(50)) &&
          index != 0) {
        --index;
        changed = true;
      }
      UnityEngine.TextAnchor old_alignment =
          UnityEngine.GUI.skin.textArea.alignment;
      UnityEngine.GUI.skin.textArea.alignment =
          UnityEngine.TextAnchor.MiddleRight;
      UnityEngine.GUILayout.TextArea(
          text    : String.Format(Culture.culture, format, array[index]),
          options : UnityEngine.GUILayout.Width(75));
      UnityEngine.GUI.skin.textArea.alignment = old_alignment;
      if (UnityEngine.GUILayout.Button(
              text    : index == array.Length - 1 ? "max" : "+",
              options : UnityEngine.GUILayout.Width(50)) &&
          index != array.Length - 1) {
        ++index;
        changed = true;
      }
    }
  }

  private void ToggleableSection(String name,
                                 ref bool show,
                                 GUIRenderer render) {
    String toggle = show ? "↑ " + name + " ↑"
                         : "↓ " + name + " ↓";
    if (UnityEngine.GUILayout.Button(toggle)) {
      show = !show;
      if (!show) {
        Shrink();
      }
    }
    if (show) {
      render();
    }
  }

  private static readonly double[] history_lengths_ =
      {1 << 10, 1 << 11, 1 << 12, 1 << 13, 1 << 14, 1 << 15, 1 << 16, 1 << 17,
       1 << 18, 1 << 19, 1 << 20, 1 << 21, 1 << 22, 1 << 23, 1 << 24, 1 << 25,
       1 << 26, 1 << 27, 1 << 28, 1 << 29, double.PositiveInfinity};
  private static readonly double[] prediction_length_tolerances_ =
      {1e-3, 1e-2, 1e0, 1e1, 1e2, 1e3, 1e4};
  private static readonly Int64[] prediction_steps_ =
      {1 << 2, 1 << 4, 1 << 6, 1 << 8, 1 << 10, 1 << 12, 1 << 14, 1 << 16,
       1 << 18, 1 << 20, 1 << 22, 1 << 24};

  private readonly FlightPlanner flight_planner_;
  private readonly ReferenceFrameSelector plotting_frame_selector_;

  private bool selecting_target_celestial_ = false;

  private bool show_ksp_features_ = false;
  private bool show_logging_settings_ = false;
  private bool show_prediction_settings_ = true;

  //TODO(phl): Compatibility code.  Yuck.
  private int prediction_length_tolerance_index_ = 1;
  private int prediction_steps_index_ = 4;
  private int history_length_index_ = 10;

  private int buffered_logging_ = 0;
  private int stderr_logging_ = 2;
  private int suppressed_logging_ = 0;
  private int verbose_logging_ = 0;

  // Whether a journal will be recorded when the plugin is next constructed.
  private bool must_record_journal_ = false;
  // Whether a journal is currently being recorded.
  private static bool journaling_ = false;

  private IntPtr plugin_ = IntPtr.Zero;
}

}  // namespace ksp_plugin_adapter
}  // namespace principia
