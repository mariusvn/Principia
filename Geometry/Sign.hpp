#pragma once

namespace principia {
namespace geometry {

// A simple abstraction for something that can take the values -1 and 1.  Useful
// for instance to represent the determinant of an orthogonal map.
class Sign {
 public:
  template<typename Scalar> explicit Sign(Scalar const& s);
  ~Sign() = default;

  bool Negative() const;
  bool Positive() const;

 private:
  bool const negative_;
  friend Sign operator*(Sign const& left, Sign const& right);
};

Sign operator*(Sign const& left, Sign const& right);

}  // namespace geometry
}  // namespace principia

#include "Geometry/Sign-body.hpp"