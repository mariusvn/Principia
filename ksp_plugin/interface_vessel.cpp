﻿
#include "ksp_plugin/interface.hpp"

#include "journal/method.hpp"
#include "journal/profiles.hpp"
#include "geometry/grassmann.hpp"
#include "geometry/named_quantities.hpp"
#include "physics/degrees_of_freedom.hpp"
#include "quantities/quantities.hpp"
#include "quantities/si.hpp"

namespace principia {
namespace interface {

using geometry::AngularVelocity;
using geometry::Displacement;
using geometry::OrthogonalMap;
using geometry::Vector;
using geometry::Velocity;
using ksp_plugin::ApparentBubble;
using ksp_plugin::Bubble;
using physics::DegreesOfFreedom;
using physics::RigidMotion;
using physics::RigidTransformation;
using quantities::Force;
using quantities::si::Kilo;
using quantities::si::Newton;
using quantities::si::Tonne;

XYZ principia__VesselBinormal(Plugin const* const plugin,
                              char const* const vessel_guid) {
  journal::Method<journal::VesselBinormal> m({plugin, vessel_guid});
  return m.Return(
      ToXYZ(CHECK_NOTNULL(plugin)->VesselBinormal(vessel_guid).coordinates()));
}

void principia__VesselClearIntrinsicForce(Plugin const* const plugin,
                                          char const* const vessel_guid) {
  journal::Method<journal::VesselClearIntrinsicForce> m({plugin, vessel_guid});
  GetVessel(*plugin, vessel_guid)->clear_intrinsic_force();
  return m.Return();
}

void principia__VesselClearMass(Plugin const* const plugin,
                                char const* const vessel_guid) {
  journal::Method<journal::VesselClearMass> m({plugin, vessel_guid});
  GetVessel(*plugin, vessel_guid)->clear_mass();
  return m.Return();
}

QP principia__VesselGetActualDegreesOfFreedom(Plugin const* const plugin,
                                              char const* const vessel_guid) {
  journal::Method<journal::VesselGetActualDegreesOfFreedom> m(
      {plugin, vessel_guid});

  RigidMotion<Bubble, World> bubble_to_world{
      RigidTransformation<Bubble, World>{
          Bubble::origin,
          World::origin,
          plugin->BarycentricToWorld() *
              OrthogonalMap<Bubble, Barycentric>::Identity()},
      AngularVelocity<Bubble>{},
      Velocity<Bubble>{}};
  auto const vessel = GetVessel(*plugin, vessel_guid);
  PileUp& pile_up = GetPileUp(*vessel);
  auto const degrees_of_freedom_in_bubble =
      pile_up.GetVesselActualDegreesOfFreedom(vessel,
                                              plugin->GetBubbleBarycentre());
  auto const degrees_of_freedom_in_world =
      bubble_to_world(degrees_of_freedom_in_bubble);
  XYZ const q = ToXYZ(
      (degrees_of_freedom_in_world.position() - World::origin).coordinates() /
      Metre);
  XYZ const p = ToXYZ(degrees_of_freedom_in_world.velocity().coordinates() /
                      (Metre / Second));
  return m.Return({q, p});
}

AdaptiveStepParameters principia__VesselGetPredictionAdaptiveStepParameters(
    Plugin const* const plugin,
    char const* const vessel_guid) {
  journal::Method<journal::VesselGetPredictionAdaptiveStepParameters> m(
      {plugin, vessel_guid});
  CHECK_NOTNULL(plugin);
  return m.Return(ToAdaptiveStepParameters(
      GetVessel(*plugin, vessel_guid)->prediction_adaptive_step_parameters()));
}

void principia__VesselIncrementIntrinsicForce(
    Plugin const* const plugin,
    char const* const vessel_guid,
    XYZ const intrinsic_force_in_kilonewtons) {
  journal::Method<journal::VesselIncrementIntrinsicForce> m(
      {plugin, vessel_guid, intrinsic_force_in_kilonewtons});
  auto const intrinsic_force_in_world = Vector<Force, World>(
      FromXYZ(intrinsic_force_in_kilonewtons) * Kilo(Newton));
  Vector<Force, Barycentric> const intrinsic_force_in_barycentric =
      plugin->WorldToBarycentric()(intrinsic_force_in_world);
  GetVessel(*plugin, vessel_guid)
      ->increment_intrinsic_force(intrinsic_force_in_barycentric);
  return m.Return();
}

void principia__VesselIncrementMass(Plugin const* const plugin,
                                    char const* const vessel_guid,
                                    double const mass_in_tonnes) {
  journal::Method<journal::VesselIncrementMass> m(
      {plugin, vessel_guid, mass_in_tonnes});
  GetVessel(*plugin, vessel_guid)->increment_mass(mass_in_tonnes * Tonne);
  return m.Return();
}

XYZ principia__VesselNormal(Plugin const* const plugin,
                            char const* const vessel_guid) {
  journal::Method<journal::VesselNormal> m({plugin, vessel_guid});
  CHECK_NOTNULL(plugin);
  return m.Return(ToXYZ(plugin->VesselNormal(vessel_guid).coordinates()));
}

void principia__VesselSetApparentDegreesOfFreedom(
    Plugin* const plugin,
    char const* const vessel_guid,
    QP const qp) {
  journal::Method<journal::VesselSetApparentDegreesOfFreedom> m(
      {plugin, vessel_guid, qp});

  RigidMotion<World, ApparentBubble> world_to_apparent_bubble(
      RigidTransformation<World, ApparentBubble>{
          World::origin,
          ApparentBubble::origin,
          OrthogonalMap<Barycentric, ApparentBubble>::Identity() *
              plugin->WorldToBarycentric()},
      AngularVelocity<World>{},
      Velocity<World>{});
  DegreesOfFreedom<World> const world_degrees_of_freedom{
      World::origin + Displacement<World>(FromXYZ(qp.q) * Metre),
      Velocity<World>(FromXYZ(qp.p) * (Metre / Second))};
  auto const vessel = GetVessel(*plugin, vessel_guid);
  PileUp& pile_up = GetPileUp(*vessel);

  plugin->AddPileUpToBubble(vessel->containing_pile_up()->iterator());
  pile_up.SetVesselApparentDegreesOfFreedom(
      vessel,
      world_to_apparent_bubble(world_degrees_of_freedom));

  return m.Return();
}

void principia__VesselSetPredictionAdaptiveStepParameters(
    Plugin const* const plugin,
    char const* const vessel_guid,
    AdaptiveStepParameters const adaptive_step_parameters) {
  journal::Method<journal::VesselSetPredictionAdaptiveStepParameters> m(
      {plugin, vessel_guid, adaptive_step_parameters});
  CHECK_NOTNULL(plugin);
  GetVessel(*plugin, vessel_guid)
      ->set_prediction_adaptive_step_parameters(
          FromAdaptiveStepParameters(adaptive_step_parameters));
  return m.Return();
}

XYZ principia__VesselTangent(Plugin const* const plugin,
                             char const* const vessel_guid) {
  journal::Method<journal::VesselTangent> m({plugin, vessel_guid});
  CHECK_NOTNULL(plugin);
  return m.Return(ToXYZ(plugin->VesselTangent(vessel_guid).coordinates()));
}

XYZ principia__VesselVelocity(Plugin const* const plugin,
                              char const* const vessel_guid) {
  journal::Method<journal::VesselVelocity> m({plugin, vessel_guid});
  CHECK_NOTNULL(plugin);
  return m.Return(ToXYZ(plugin->VesselVelocity(vessel_guid).coordinates() /
                        (Metre / Second)));
}

}  // namespace interface
}  // namespace principia
