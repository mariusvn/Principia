﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// This namespace is a monstrosity as far as code replication is concerned, but
// there is no way around that short of rewriting the whole thing in C++/CLI.
// This might be done eventually (especially if we need to interface with native
// C++), but is not a high priority. It would make the management of physical
// quantities easier and more systematic.
namespace NewtonianPhysics.Geometry {
  // Structs for strong typing in a three dimensional real inner product space
  // and its Grassmann algebra, as well as an affine space.
  // Double-precision floating point numbers, representing reals, are wrapped in
  // the struct Scalar to ensure strong typing. Three-dimensional data is stored
  // as R3Element.
  // We use the Grassman algebra in order to avoid the confusion between pseudo-
  // vectors and vectors and between pseudoscalars and scalars. This allows for
  // handling of indirect changes of coordinates. As a result, there is no cross
  // product except for the underlying data type R3Element.
  // We treat the isomorphism between a space and its dual as implicit, as we
  // are dealing with inner product spaces.
  // TODO(robin): We may want to change this when we get to rotating-pulsating
  // reference frames.
  // A reminder of the equivalents in the Grassman algebra of the cross
  // product; We identify V ^ V with the Lie algebra so(V).
  // . pseudovector = vector x vector, as in L = r x p, becomes the wedge
  //   product,
  //   bivector = vector ^ vector, L = r ^ p.
  // . vector = pseudovector x vector, as in a_Coriolis = 2 Ω x v, becomes
  //   the left action of the bivector,
  //   vector = bivector . vector,  a_Coriolis = 2 Ω . v.
  // . vector = vector x pseudovector, as in F = q v x B, becomes
  //   the dual action,
  //   vector = bivector . vector,  F* = q v* . B.
  // . pseudovector = pseudovector x pseudovector, as in the composition of
  //   rotation vectors Θ12 = α Θ1 + β Θ2 + γ Θ1 x Θ2, becomes the Lie bracket
  //   of so(V).
  //   bivector = [bivector, bivector], Θ12 = α Θ1 + β Θ2 + γ [Θ1, Θ2].
  // . pseudoscalar = (pseudovector|vector), as in Φ = (B|S), becomes the wedge
  //   product,
  //   trivector = bivector ^ vector, Φ = B ^ S.
  // NOTATION:
  // For k-vectors v, w, the inner product (v|w) is [k]vector.InnerProduct(v, w)
  // rather than v.InnerProduct(w) so that the enclosing space is explicit.
  // Compare with the notation v.Wedge(w) for the exterior product of a k-vector
  // v and a l-vector w.
  // We choose to use operators for the vector space operations, thus keeping
  // the enclosing space implicit, i.e., v + w rather than [k]vector.Plus(v, w).
  // The left action a . v of a bivector a on v is a.ActOn(v).
  // The right action v* . a is v.ActedUponBy(a). The commutator is
  // Bivector.Commutator(a, b).

  public interface IAffineSpace { };

  public struct Sign {
    public bool positive;
    public static explicit operator Sign(Scalar x) {
      return new Sign { positive = x > (Scalar)0 };
    }
    public static implicit operator Scalar(Sign sgn) {
      return sgn.positive ? (Scalar)1 : (Scalar)(-1);
    }
    public static Sign operator *(Sign left, Sign right) {
      return new Sign { positive = (left.positive == right.positive) };
    }
  }

  #region Underlying weakly-typed vector

  public struct R3Element {
    public Scalar x, y, z;
    public static R3Element operator -(R3Element v) {
      return new R3Element { x = v.x, y = v.y, z = v.z };
    }
    public static R3Element operator -(R3Element left, R3Element right) {
      return new R3Element {
        x = left.x - right.x,
        y = left.y - right.y,
        z = left.z - right.z
      };
    }
    public static R3Element operator *(Scalar left, R3Element right) {
      return new R3Element {
        x = left * right.x,
        y = left * right.y,
        z = left * right.z
      };
    }
    public static R3Element operator *(R3Element left, Scalar right) {
      return new R3Element {
        x = left.x * right,
        y = left.y * right,
        z = left.z * right
      };
    }
    public static R3Element operator /(R3Element left, Scalar right) {
      return new R3Element {
        x = left.x / right,
        y = left.y / right,
        z = left.z / right
      };
    }
    public static R3Element operator +(R3Element left, R3Element right) {
      return new R3Element {
        x = left.x + right.x,
        y = left.y + right.y,
        z = left.z + right.z
      };
    }
    public R3Element Cross(R3Element right) {
      return new R3Element {
        x = this.y * right.z - this.z * right.y,
        y = this.z * right.x - this.x * right.z,
        z = this.x * right.y - this.y * right.x
      };
    }
    public Scalar Dot(R3Element right) {
      return this.x * right.x + this.y * right.y + this.z * right.z;
    }
  }

  #endregion Underlying weakly-typed vector

  #region Grassman algebra

  public struct Scalar : IComparable, IComparable<Scalar>, IEquatable<Scalar> {
    private double value;
    public static Scalar Cos(Scalar angle) {
      return (Scalar)Math.Cos((double)angle);
    }
    public static explicit operator double(Scalar x) { return x.value; }
    public static explicit operator Scalar(double x) {
      return new Scalar { value = x };
    }
    public static Scalar operator -(Scalar x) {
      return (Scalar)(-(double)x);
    }
    public static Scalar operator -(Scalar x, Scalar y) {
      return (Scalar)((double)x - (double)y);
    }
    public static bool operator !=(Scalar x, Scalar y) {
      return (double)x != (double)y;
    }
    public static Scalar operator *(Scalar x, Scalar y) {
      return (Scalar)((double)x * (double)y);
    }
    public static Scalar operator /(Scalar x, Scalar y) {
      return (Scalar)((double)x / (double)y);
    }
    public static Scalar operator +(Scalar x, Scalar y) {
      return (Scalar)((double)x + (double)y);
    }
    public static bool operator <(Scalar x, Scalar y) {
      return (double)x < (double)y;
    }
    public static bool operator <=(Scalar x, Scalar y) {
      return (double)x <= (double)y;
    }
    public static bool operator ==(Scalar x, Scalar y) {
      return (double)x == (double)y;
    }
    public static bool operator >(Scalar x, Scalar y) {
      return (double)x > (double)y;
    }
    public static bool operator >=(Scalar x, Scalar y) {
      return (double)x >= (double)y;
    }
    public static Scalar Sin(Scalar angle) {
      return (Scalar)Math.Sin((double)angle);
    }
    public static Scalar Sqrt(Scalar angle) {
      return (Scalar)Math.Sqrt((double)angle);
    }
    public static Scalar Tan(Scalar angle) {
      return (Scalar)Math.Tan((double)angle);
    }
    public int CompareTo(object obj) {
      if (obj == null) {
        // MSDN: By definition, any object compares greater than (or follows)
        // null, and two null references compare equal to each other.
        return 1;
      } else if (!this.GetType().Equals(obj.GetType())) {
        throw new ArgumentException("Object is not a Scalar");
      } else {
        return this.value.CompareTo(obj);
      }
    }
    public int CompareTo(Scalar x) {
      return this.value.CompareTo(x.value);
    }
    public override bool Equals(object obj) {
      if ((obj == null) || !this.GetType().Equals(obj.GetType())) {
        return false;
      } else {
        return this == (Scalar)obj;
      }
    }
    public bool Equals(Scalar x) {
      return this == x;
    }
    public override int GetHashCode() {
      return this.value.GetHashCode();
    }
  }
  public struct Vector<A> where A : IAffineSpace {
    public R3Element coordinates;
    public static Scalar InnerProduct(Vector<A> left, Vector<A> right) {
      return left.coordinates.Dot(right.coordinates);
    }
    public static Vector<A> operator -(Vector<A> v) {
      return new Vector<A> { coordinates = -v.coordinates };
    }
    public static Vector<A> operator -(Vector<A> left, Vector<A> right) {
      return new Vector<A> {
        coordinates = left.coordinates - right.coordinates
      };
    }
    public static Vector<A> operator *(Scalar left, Vector<A> right) {
      return new Vector<A> { coordinates = left * right.coordinates };
    }
    public static Vector<A> operator *(Vector<A> left, Scalar right) {
      return new Vector<A> { coordinates = left.coordinates * right };
    }
    public static Vector<A> operator /(Vector<A> left, Scalar right) {
      return new Vector<A> { coordinates = left.coordinates / right };
    }
    public static Vector<A> operator +(Vector<A> left, Vector<A> right) {
      return new Vector<A> {
        coordinates = left.coordinates + right.coordinates
      };
    }
    public static Vector<A> ToFrom(Point<A> left, Point<A> right) {
      return new Vector<A> {
        coordinates = left.coordinates - right.coordinates
      };
    }
    public Vector<A> ActedUponBy(BiVector<A> right) {
      return new Vector<A> {
        coordinates = this.coordinates.Cross(right.coordinates)
      };
    }
    public Point<A> Translate(Point<A> right) {
      return new Point<A> {
        coordinates = this.coordinates + right.coordinates
      };
    }
    public BiVector<A> Wedge(Vector<A> right) {
      return new BiVector<A> {
        coordinates = this.coordinates.Cross(right.coordinates)
      };
    }
    public TriVector<A> Wedge(BiVector<A> right) {
      return new TriVector<A> {
        coordinate = this.coordinates.Dot(right.coordinates)
      };
    }
  }
  public struct BiVector<A> where A : IAffineSpace {
    public R3Element coordinates;
    public static BiVector<A> Commutator(BiVector<A> left, BiVector<A> right) {
      return new BiVector<A> {
        coordinates = left.coordinates.Cross(right.coordinates)
      };
    }
    public static Rotation<A, A> Exp(BiVector<A> infinitesimalRotation) {
      Scalar angle = Scalar.Sqrt(BiVector<A>.InnerProduct(infinitesimalRotation,
                                                       infinitesimalRotation));
      return new Rotation<A, A> {
        realPart = Scalar.Cos(angle / (Scalar)2),
        imaginaryPart = infinitesimalRotation.coordinates / angle
                        * Scalar.Sin(angle / (Scalar)2)
      };
    }
    public static Scalar InnerProduct(BiVector<A> left, BiVector<A> right) {
      return left.coordinates.Dot(right.coordinates);
    }
    public static BiVector<A> operator -(BiVector<A> v) {
      return new BiVector<A> { coordinates = -v.coordinates };
    }
    public static BiVector<A> operator -(BiVector<A> left, BiVector<A> right) {
      return new BiVector<A> {
        coordinates = left.coordinates - right.coordinates
      };
    }
    public static BiVector<A> operator *(Scalar left, BiVector<A> right) {
      return new BiVector<A> { coordinates = left * right.coordinates };
    }
    public static BiVector<A> operator *(BiVector<A> left, Scalar right) {
      return new BiVector<A> { coordinates = left.coordinates * right };
    }
    public static BiVector<A> operator /(BiVector<A> left, Scalar right) {
      return new BiVector<A> { coordinates = left.coordinates / right };
    }
    public static BiVector<A> operator +(BiVector<A> left, BiVector<A> right) {
      return new BiVector<A> {
        coordinates = left.coordinates + right.coordinates
      };
    }
    public Vector<A> ActOn(Vector<A> right) {
      return new Vector<A> {
        coordinates = this.coordinates.Cross(right.coordinates)
      };
    }
    public TriVector<A> Wedge(Vector<A> right) {
      return new TriVector<A> {
        coordinate = this.coordinates.Dot(right.coordinates)
      };
    }
  }
  public struct TriVector<A> where A : IAffineSpace {
    public Scalar coordinate;
  }

  #endregion Grassman algebra

  #region Affine space

  public struct Point<A> where A : IAffineSpace {
    public R3Element coordinates;
    // A convex combination of positions is a position.
    public static Point<A> Barycenter(Point<A> q1, Scalar λ1,
                                      Point<A> q2, Scalar λ2) {
      return new Point<A> {
        coordinates = (q1.coordinates * λ1 + q2.coordinates * λ2) / (λ1 + λ2)
      };
    }
  }

  #endregion Affine space

  #region Maps between base spaces

  public struct Rotation<A, B>
    where A : IAffineSpace
    where B : IAffineSpace {
    // The rotation is modeled as a quaternion.
    public Scalar realPart;
    public R3Element imaginaryPart;
    public Rotation<B, A> Inverse {
      get {
        return new Rotation<B, A> {
          realPart = realPart,
          imaginaryPart = -imaginaryPart
        };
      }
    }
    public static R3Element operator *(Rotation<A, B> left,
                                       R3Element right) {
      // TODO(egg): Optimise.
      return Maps.Compose<B, A, B>(
        Maps.Compose<A, A, B>(
          left,
          new Rotation<A, A> { realPart = (Scalar)0, imaginaryPart = right }),
        left.Inverse).imaginaryPart;
    }
    public Vector<B> ActOn(Vector<A> right) {
      return new Vector<B> { coordinates = this * right.coordinates };
    }
  }
  public struct OrthogonalTransformation<A, B>
    where A : IAffineSpace
    where B : IAffineSpace {
    // The orthogonal transformation is modeled as a rotoinversion.
    public Sign determinant;
    public Rotation<A, B> specialOrthogonalMap;

    public static R3Element operator *(OrthogonalTransformation<A, B> left,
                                      R3Element right) {
      return left.specialOrthogonalMap * (left.determinant * right);
    }
    public Vector<B> ActOn(Vector<A> right) {
      return new Vector<B> { coordinates = this * right.coordinates };
    }
    public BiVector<A> ActOn(BiVector<A> right) {
      return new BiVector<A> {
        coordinates = this.specialOrthogonalMap * right.coordinates
      };
    }
    public TriVector<A> ActOn(TriVector<A> right) {
      return new TriVector<A> {
        coordinate = this.determinant * right.coordinate
      };
    }
  }
  public struct EuclideanTransformation<A, B>
    where A : IAffineSpace
    where B : IAffineSpace {
    public OrthogonalTransformation<A, B> orthogonalMap;
    public Vector<B> translation;
  }

  #endregion Maps between base spaces

  #region Methods for the composition of maps

  public static class Maps {
    public static Rotation<A, C> Compose<A, B, C>(Rotation<B, C> left,
                                                  Rotation<A, B> right)
      where A : IAffineSpace
      where B : IAffineSpace
      where C : IAffineSpace {
      return new Rotation<A, C> {
        realPart = left.realPart * right.realPart
                   - left.imaginaryPart.Dot(right.imaginaryPart),
        imaginaryPart = left.realPart * right.imaginaryPart
                        + right.realPart * left.imaginaryPart
                        + left.imaginaryPart.Cross(right.imaginaryPart)
      };
    }
    public static OrthogonalTransformation<A, C> Compose<A, B, C>(
  OrthogonalTransformation<B, C> left,
  OrthogonalTransformation<A, B> right)
      where A : IAffineSpace
      where B : IAffineSpace
      where C : IAffineSpace {
      return new OrthogonalTransformation<A, C> {
        determinant = left.determinant * right.determinant,
        specialOrthogonalMap = Compose<A, B, C>(left.specialOrthogonalMap,
                                                right.specialOrthogonalMap)
      };
    }
    public static EuclideanTransformation<A, C> Compose<A, B, C>(
  EuclideanTransformation<B, C> left,
  EuclideanTransformation<A, B> right)
      where A : IAffineSpace
      where B : IAffineSpace
      where C : IAffineSpace {
      return new EuclideanTransformation<A, C> {
        orthogonalMap = Compose<A, B, C>(left.orthogonalMap,
                                         right.orthogonalMap),
        translation = left.translation
                      + left.orthogonalMap.ActOn(right.translation)
      };
    }
  }

  #endregion Methods for the composition of maps
}