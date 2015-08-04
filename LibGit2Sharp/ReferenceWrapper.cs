using System;
using System.Diagnostics;
using System.Globalization;
using LibGit2Sharp.Core;

namespace LibGit2Sharp
{
    /// <summary>
    ///
    /// </summary>
    public abstract class BaseReferenceWrapper : IBelongToARepository, IEquatable<BaseReferenceWrapper>
    {
        private static readonly LambdaEqualityHelper<BaseReferenceWrapper> equalityHelper =
            new LambdaEqualityHelper<BaseReferenceWrapper>(x => x.CanonicalName, x => x.TargetObject);

        /// <summary>
        /// The repository.
        /// </summary>
        protected readonly Repository repo;
        private readonly string canonicalName;
        private readonly Lazy<GitObject> objectBuilder;

        /// <summary>
        /// Needed for mocking purposes.
        /// </summary>
        protected BaseReferenceWrapper()
        { }

        /// <summary>
        ///
        /// </summary>
        /// <param name="repo"></param>
        /// <param name="reference"></param>
        /// <param name="canonicalNameSelector"></param>
        protected internal BaseReferenceWrapper(Repository repo, Reference reference, Func<Reference, string> canonicalNameSelector)
        {
            Ensure.ArgumentNotNull(repo, "repo");
            Ensure.ArgumentNotNull(reference, "reference");
            Ensure.ArgumentNotNull(canonicalNameSelector, "canonicalNameSelector");

            this.repo = repo;
            Reference = reference;
            canonicalName = canonicalNameSelector(reference);
            this.objectBuilder = new Lazy<GitObject>(RetrieveTargetObject);
        }

        /// <summary>
        ///
        /// </summary>
        public Reference Reference { get;}

        /// <summary>
        /// Gets the full name of this reference.
        /// </summary>
        public virtual string CanonicalName
        {
            get { return canonicalName; }
        }

        /// <summary>
        /// Gets the human-friendly name of this reference.
        /// </summary>
        public virtual string FriendlyName
        {
            get { return Shorten(); }
        }

        /// <summary>
        /// The current id of the object the reference points to
        /// </summary>
        public ObjectId CurrentId => Reference.ResolveToDirectReference().Id;

        /// <summary>
        /// Returns the <see cref="CanonicalName"/>, a <see cref="string"/> representation of the current reference.
        /// </summary>
        /// <returns>The <see cref="CanonicalName"/> that represents the current reference.</returns>
        public override string ToString()
        {
            return CanonicalName;
        }

        /// <summary>
        /// Gets the Object this <see cref="ReferenceWrapper{TObject}"/> points to.
        /// </summary>
        public GitObject TargetObject
        {
            get { return objectBuilder.Value; }
        }

        /// <summary>
        /// creates the Object this ReferenceWrapper points to.
        /// </summary>
        /// <returns></returns>
        protected virtual GitObject RetrieveTargetObject()
        {
            var directReference = Reference.ResolveToDirectReference();
            if (directReference == null)
            {
                return null;
            }

            var target = directReference.Target;
            if (target == null)
            {
                return null;
            }

            return repo.Lookup(target.Id);
        }

        /// <summary>
        /// Removes redundent leading namespaces (regarding the kind of
        /// reference being wrapped) from the canonical name.
        /// </summary>
        /// <returns>The friendly shortened name</returns>
        protected abstract string Shorten();


        /// <summary>
        /// Determines whether the specified <see cref="ReferenceWrapper{TObject}"/> is equal to the current <see cref="ReferenceWrapper{TObject}"/>.
        /// </summary>
        /// <param name="other">The <see cref="ReferenceWrapper{TObject}"/> to compare with the current <see cref="ReferenceWrapper{TObject}"/>.</param>
        /// <returns>True if the specified <see cref="ReferenceWrapper{TObject}"/> is equal to the current <see cref="ReferenceWrapper{TObject}"/>; otherwise, false.</returns>
        public bool Equals(BaseReferenceWrapper other)
        {
            return equalityHelper.Equals(this, other);
        }

        /// <summary>
        /// Determines whether the specified <see cref="Object"/> is equal to the current <see cref="ReferenceWrapper{TObject}"/>.
        /// </summary>
        /// <param name="obj">The <see cref="Object"/> to compare with the current <see cref="ReferenceWrapper{TObject}"/>.</param>
        /// <returns>True if the specified <see cref="Object"/> is equal to the current <see cref="ReferenceWrapper{TObject}"/>; otherwise, false.</returns>
        public override bool Equals(object obj)
        {
            return Equals(obj as BaseReferenceWrapper);
        }

        /// <summary>
        /// Returns the hash code for this instance.
        /// </summary>
        /// <returns>A 32-bit signed integer hash code.</returns>
        public override int GetHashCode()
        {
            return equalityHelper.GetHashCode(this);
        }

        /// <summary>
        /// Tests if two <see cref="ReferenceWrapper{TObject}"/> are equal.
        /// </summary>
        /// <param name="left">First <see cref="ReferenceWrapper{TObject}"/> to compare.</param>
        /// <param name="right">Second <see cref="ReferenceWrapper{TObject}"/> to compare.</param>
        /// <returns>True if the two objects are equal; false otherwise.</returns>
        public static bool operator ==(BaseReferenceWrapper left, BaseReferenceWrapper right)
        {
            return Equals(left, right);
        }

        /// <summary>
        /// Tests if two <see cref="ReferenceWrapper{TObject}"/> are different.
        /// </summary>
        /// <param name="left">First <see cref="ReferenceWrapper{TObject}"/> to compare.</param>
        /// <param name="right">Second <see cref="ReferenceWrapper{TObject}"/> to compare.</param>
        /// <returns>True if the two objects are different; false otherwise.</returns>
        public static bool operator !=(BaseReferenceWrapper left, BaseReferenceWrapper right)
        {
            return !Equals(left, right);
        }

        IRepository IBelongToARepository.Repository => repo;
    }

    /// <summary>
    /// A base class for things that wrap a <see cref="Reference"/> (branch, tag, etc).
    /// </summary>
    /// <typeparam name="TObject">The type of the referenced Git object.</typeparam>
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    public abstract class ReferenceWrapper<TObject> : BaseReferenceWrapper where TObject : GitObject
    {

        /// <summary>
        /// Needed for mocking purposes.
        /// </summary>
        protected ReferenceWrapper()
        { }

        /// <param name="repo">The repository.</param>
        /// <param name="reference">The reference.</param>
        /// <param name="canonicalNameSelector">A function to construct the reference's canonical name.</param>
        protected internal ReferenceWrapper(Repository repo, Reference reference, Func<Reference, string> canonicalNameSelector)
            :base(repo, reference, canonicalNameSelector)
        {
        }

        /// <summary>
        /// Gets the name of this reference.
        /// </summary>
        [Obsolete("This property will be removed in the next release. Please use FriendlyName instead.")]
        public virtual string Name
        {
            get { return FriendlyName; }
        }

        /// <summary>
        /// Gets the <typeparamref name="TObject"/> this <see cref="ReferenceWrapper{TObject}"/> points to.
        /// </summary>
        protected new TObject TargetObject
        {
            get { return (TObject)base.TargetObject; }
        }

        /// <summary>
        ///
        /// </summary>
        /// <returns></returns>
        protected override GitObject RetrieveTargetObject()
        {
            var directReference = Reference.ResolveToDirectReference();
            if (directReference == null)
            {
                return null;
            }

            var target = directReference.Target;
            if (target == null)
            {
                return null;
            }

            return repo.Lookup<TObject>(target.Id);
        }

        private string DebuggerDisplay
        {
            get
            {
                return string.Format(CultureInfo.InvariantCulture,
                                     "{0} => \"{1}\"", CanonicalName,
                                     (TargetObject != null)
                                        ? TargetObject.Id.ToString(7)
                                        : "?");
            }
        }
    }
}
