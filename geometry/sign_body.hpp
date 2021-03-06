﻿
#pragma once

#include "geometry/sign.hpp"

#include <string>

#include "base/macros.hpp"

namespace principia {
namespace geometry {
namespace internal_sign {

template<typename Scalar>
Sign::Sign(Scalar const& scalar) : negative_(scalar < Scalar{}) {}

inline bool Sign::Negative() const {
  return negative_;
}

inline bool Sign::Positive() const {
  return !negative_;
}

inline bool Sign::operator==(Sign const& other) const {
  return negative_ == other.negative_;
}

inline bool Sign::operator!=(Sign const& other) const {
  return negative_ != other.negative_;
}

inline void Sign::WriteToMessage(
    not_null<serialization::Sign*> const message) const {
  message->set_negative(negative_);
}

inline Sign Sign::ReadFromMessage(serialization::Sign const& message) {
  return Sign(message.negative() ? -1 : 1);
}

inline Sign operator*(Sign const& left, Sign const& right) {
  return Sign(left.negative_ == right.negative_ ? 1 : -1);
}

template<typename T>
FORCE_INLINE(inline) T operator*(Sign const& left, T const& right) {
  return left.negative_ ? -right : right;
}

inline std::string DebugString(Sign const& sign) {
  return sign.Negative() ? "-" : "+";
}

inline std::ostream& operator<<(std::ostream& out, Sign const& sign) {
  out << DebugString(sign);
  return out;
}

}  // namespace internal_sign
}  // namespace geometry
}  // namespace principia
