#pragma once

#include <map>
#include <utility>

#include "geometry/grassmann.hpp"
#include "geometry/linear_map.hpp"
#include "geometry/quaternion.hpp"
#include "geometry/r3_element.hpp"
#include "geometry/sign.hpp"
#include "quantities/dimensionless.hpp"
#include "quantities/elementary_functions.hpp"

namespace principia {
namespace geometry {

template<typename FromFrame, typename ToFrame>
Permutation<FromFrame, ToFrame>::Permutation(
    CoordinatePermutation const coordinate_permutation)
    : coordinate_permutation_(coordinate_permutation) {}

template<typename FromFrame, typename ToFrame>
inline Sign Permutation<FromFrame, ToFrame>::Determinant() const {
  return Sign(coordinate_permutation_);
}

template<typename FromFrame, typename ToFrame>
Permutation<ToFrame, FromFrame>
Permutation<FromFrame, ToFrame>::Inverse() const {
  static std::map<CoordinatePermutation const,
                  CoordinatePermutation const> const inverse = {
      {XYZ, XYZ},
      {YZX, ZXY},
      {ZXY, YZX},
      {XZY, XZY},
      {ZYX, ZYX},
      {YXZ, YXZ}};
  return Permutation(inverse.at(coordinate_permutation_));
}

template<typename FromFrame, typename ToFrame>
template<typename Scalar>
Vector<Scalar, ToFrame> Permutation<FromFrame, ToFrame>::operator()(
    Vector<Scalar, FromFrame> const& vector) const {
  return Vector<Scalar, ToFrame>((*this)(vector.coordinates()));
}

template<typename FromFrame, typename ToFrame>
template<typename Scalar>
Bivector<Scalar, ToFrame> Permutation<FromFrame, ToFrame>::operator()(
    Bivector<Scalar, FromFrame> const& bivector) const {
  return Bivector<Scalar, ToFrame>(
      Determinant() * (*this)(bivector.coordinates()));
}

template<typename FromFrame, typename ToFrame>
template<typename Scalar>
Trivector<Scalar, ToFrame> Permutation<FromFrame, ToFrame>::operator()(
    Trivector<Scalar, FromFrame> const& trivector) const {
  return Trivector<Scalar, ToFrame>(Determinant() * trivector.coordinates());
}

template<typename FromFrame, typename ToFrame>
OrthogonalMap<FromFrame, ToFrame>
Permutation<FromFrame, ToFrame>::Forget() const {
  static quantities::Dimensionless const sqrt_half = quantities::Sqrt(0.5);
  static std::map<CoordinatePermutation const,
                  Quaternion const> const quaternion = {
      {XYZ, Quaternion(1, {0, 0, 0})},
      {YZX, Quaternion(0.5, {-0.5, -0.5, -0.5})},
      {ZXY, Quaternion(0.5, {0.5, 0.5, 0.5})},
      {XZY, Quaternion(0, {0, -sqrt_half, sqrt_half})},
      {ZYX, Quaternion(0, {-sqrt_half, 0, sqrt_half})},
      {YXZ, Quaternion(0, {-sqrt_half, sqrt_half, 0})}};
  return OrthogonalMap<FromFrame, ToFrame>(
      Determinant(),
      Rotation<FromFrame, ToFrame>(quaternion.at(coordinate_permutation_)));
}

template<typename FromFrame, typename ToFrame>
Permutation<FromFrame, ToFrame> Permutation<FromFrame, ToFrame>::Identity() {
  return Permutation(XYZ);
}

template<typename FromFrame, typename ToFrame>
template<typename Scalar>
R3Element<Scalar> Permutation<FromFrame, ToFrame>::operator()(
    R3Element<Scalar> const& r3_element) const {
  R3Element<Scalar> result;
  for (int coordinate = x; coordinate <= z; ++coordinate) {
    result[coordinate] = r3_element[
        0x3 &
        (static_cast<int>(coordinate_permutation_) >> (coordinate * 2))];
  }
  return result;
}

template<typename FromFrame, typename ThroughFrame, typename ToFrame>
Permutation<FromFrame, ToFrame> operator*(
    Permutation<ThroughFrame, ToFrame> const& left,
    Permutation<FromFrame, ThroughFrame> const& right) {
  typedef Permutation<FromFrame, ThroughFrame> P;

  // The pair<> is in diagrammatic order: right is applied first and is the
  // first element of the pair, left is applied second and is the second
  // element.
  static std::map<std::pair<P::CoordinatePermutation const,
                            P::CoordinatePermutation const>,
                  P::CoordinatePermutation> const multiplication = {
      {{P::XYZ, P::XYZ}, P::XYZ},
      {{P::XYZ, P::YZX}, P::YZX},
      {{P::XYZ, P::ZXY}, P::ZXY},
      {{P::XYZ, P::XZY}, P::XZY},
      {{P::XYZ, P::ZYX}, P::ZYX},
      {{P::XYZ, P::YXZ}, P::YXZ},

      {{P::YZX, P::XYZ}, P::YZX},
      {{P::YZX, P::YZX}, P::ZXY},
      {{P::YZX, P::ZXY}, P::XYZ},
      {{P::YZX, P::XZY}, P::YXZ},
      {{P::YZX, P::ZYX}, P::XZY},
      {{P::YZX, P::YXZ}, P::ZYX},

      {{P::ZXY, P::XYZ}, P::ZXY},
      {{P::ZXY, P::YZX}, P::XYZ},
      {{P::ZXY, P::ZXY}, P::YZX},
      {{P::ZXY, P::XZY}, P::ZYX},
      {{P::ZXY, P::ZYX}, P::YXZ},
      {{P::ZXY, P::YXZ}, P::XZY},

      {{P::XZY, P::XYZ}, P::XZY},
      {{P::XZY, P::YZX}, P::ZYX},
      {{P::XZY, P::ZXY}, P::YXZ},
      {{P::XZY, P::XZY}, P::XYZ},
      {{P::XZY, P::ZYX}, P::YZX},
      {{P::XZY, P::YXZ}, P::ZXY},

      {{P::ZYX, P::XYZ}, P::ZYX},
      {{P::ZYX, P::YZX}, P::YXZ},
      {{P::ZYX, P::ZXY}, P::XZY},
      {{P::ZYX, P::XZY}, P::ZXY},
      {{P::ZYX, P::ZYX}, P::XYZ},
      {{P::ZYX, P::YXZ}, P::YZX},

      {{P::YXZ, P::XYZ}, P::YXZ},
      {{P::YXZ, P::YZX}, P::XZY},
      {{P::YXZ, P::ZXY}, P::ZYX},
      {{P::YXZ, P::XZY}, P::YZX},
      {{P::YXZ, P::ZYX}, P::ZXY},
      {{P::YXZ, P::YXZ}, P::XYZ}};
  return Permutation<FromFrame, ToFrame>(
      multiplication.at({right.coordinate_permutation_,
                         left.coordinate_permutation_}));
}

}  // namespace geometry
}  // namespace principia
