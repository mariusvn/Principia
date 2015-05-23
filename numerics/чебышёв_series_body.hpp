﻿
#include "numerics/чебышёв_series.hpp"

#include "glog/logging.h"
#include "numerics/arrays.hpp"
#include "numerics/newhall.mathematica.cpp"
#include "quantities/serialization.hpp"

namespace principia {

using quantities::QuantityOrDoubleSerializer;

namespace numerics {

template<typename Scalar>
ЧебышёвSeries<Scalar>::ЧебышёвSeries(std::vector<Scalar> const& coefficients,
                                     Instant const& t_min,
                                     Instant const& t_max)
    : coefficients_(coefficients),
      degree_(static_cast<int>(coefficients_.size()) - 1),
      t_min_(t_min),
      t_max_(t_max) {
  CHECK_LE(0, degree_) << "Degree must be at least 0";
  CHECK_LT(t_min_, t_max_) << "Time interval must not be empty";
  // Precomputed to save operations at the expense of some accuracy loss.
  Time const duration = t_max_ - t_min_;
  t_mean_ = t_min_ + 0.5 * duration;
  two_over_duration_ = 2 / duration;
}

template<typename Scalar>
bool ЧебышёвSeries<Scalar>::operator==(ЧебышёвSeries const& right) const {
  return coefficients_ == right.coefficients_ &&
         t_min_ == right.t_min_ &&
         t_max_ == right.t_max_;
}

template<typename Scalar>
bool ЧебышёвSeries<Scalar>::operator!=(ЧебышёвSeries const& right) const {
  return !ЧебышёвSeries<Scalar>::operator==(right);
}

template<typename Scalar>
Scalar ЧебышёвSeries<Scalar>::Evaluate(Instant const& t) const {
  double const scaled_t = (t - t_mean_) * two_over_duration_;
  double const two_scaled_t = scaled_t + scaled_t;
  // We have to allow |scaled_t| to go slightly out of [-1, 1] because of
  // computation errors.  But if it goes too far, something is broken.
  CHECK_LE(scaled_t, 1.1);
  CHECK_GE(scaled_t, -1.1);

  Scalar b_kplus2{};
  Scalar b_kplus1{};
  Scalar b_k{};
  for (int k = degree_; k >= 1; --k) {
    b_k = coefficients_[k] + two_scaled_t * b_kplus1 - b_kplus2;
    b_kplus2 = b_kplus1;
    b_kplus1 = b_k;
  }
  return coefficients_[0] + scaled_t * b_kplus1 - b_kplus2;
}

template<typename Scalar>
void ЧебышёвSeries<Scalar>::WriteToMessage(
    not_null<serialization::ЧебышёвSeries*> const message) const {
  using Serializer =
      QuantityOrDoubleSerializer<Scalar,
                                 serialization::ЧебышёвSeries::Coefficient>;
  for (auto const& coefficient : coefficients_) {
    Serializer::WriteToMessage(coefficient, message->add_coefficient());
  }
  t_min_.WriteToMessage(message->mutable_t_min());
  t_max_.WriteToMessage(message->mutable_t_max());
}

template<typename Scalar>
ЧебышёвSeries<Scalar> ЧебышёвSeries<Scalar>::ReadFromMessage(
    serialization::ЧебышёвSeries const& message) {
  using Serializer =
      QuantityOrDoubleSerializer<Scalar,
                                 serialization::ЧебышёвSeries::Coefficient>;
  std::vector<Scalar> coefficients;
  coefficients.reserve(message.coefficient_size());
  for (auto const& coefficient : message.coefficient()) {
    coefficients.push_back(Serializer::ReadFromMessage(coefficient));
  }
  return ЧебышёвSeries(coefficients,
                       Instant::ReadFromMessage(message.t_min()),
                       Instant::ReadFromMessage(message.t_max()));
}

template<typename Scalar>
ЧебышёвSeries<Scalar> ЧебышёвSeries<Scalar>::NewhallApproximation(
    int const degree,
    std::vector<Scalar> const& p,
    std::vector<Variation<Scalar>> const& v,
    Instant const& t_min,
    Instant const& t_max) {
  // Only supports 8 divisions for now.
  int const kDivisions = 8;
  CHECK_EQ(kDivisions + 1, p.size());
  CHECK_EQ(kDivisions + 1, v.size());

  Time const duration_over_two = 0.5 * (t_max - t_min);

  FixedVector<Scalar, 2 * kDivisions + 2> pv;
  for (int i = 0, j = 2 * kDivisions;
       i < kDivisions + 1 && j >= 0;
       ++i, j -= 2) {
    pv[j] = p[i];
    pv[j + 1] = v[i] * duration_over_two;
  }

  std::vector<Scalar> coefficients;
  coefficients.reserve(degree);
  switch (degree) {
    case 3:
      coefficients = newhall_c_matrix_degree_3_divisions_8_w04 * pv;
      break;
    case 4:
      coefficients = newhall_c_matrix_degree_4_divisions_8_w04 * pv;
      break;
    case 5:
      coefficients = newhall_c_matrix_degree_5_divisions_8_w04 * pv;
      break;
    case 6:
      coefficients = newhall_c_matrix_degree_6_divisions_8_w04 * pv;
      break;
    case 7:
      coefficients = newhall_c_matrix_degree_7_divisions_8_w04 * pv;
      break;
    case 8:
      coefficients = newhall_c_matrix_degree_8_divisions_8_w04 * pv;
      break;
    case 9:
      coefficients = newhall_c_matrix_degree_9_divisions_8_w04 * pv;
      break;
    case 10:
      coefficients = newhall_c_matrix_degree_10_divisions_8_w04 * pv;
      break;
    case 11:
      coefficients = newhall_c_matrix_degree_11_divisions_8_w04 * pv;
      break;
    case 12:
      coefficients = newhall_c_matrix_degree_12_divisions_8_w04 * pv;
      break;
    case 13:
      coefficients = newhall_c_matrix_degree_13_divisions_8_w04 * pv;
      break;
    case 14:
      coefficients = newhall_c_matrix_degree_14_divisions_8_w04 * pv;
      break;
    case 15:
      coefficients = newhall_c_matrix_degree_15_divisions_8_w04 * pv;
      break;
    case 16:
      coefficients = newhall_c_matrix_degree_16_divisions_8_w04 * pv;
      break;
    case 17:
      coefficients = newhall_c_matrix_degree_17_divisions_8_w04 * pv;
      break;
    default:
      break;
  }
  CHECK_EQ(degree + 1, coefficients.size());
  return ЧебышёвSeries(coefficients, t_min, t_max);
}

}  // namespace numerics
}  // namespace principia
