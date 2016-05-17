using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using FluentValidation.Results;
using FluentValidation.Validators;

namespace DevSharp.Domain
{
    public class IsAllRightValidator : IValidator
    {
        public static readonly IsAllRightValidator Default = new IsAllRightValidator();


        public ValidationResult Validate(object instance)
        {
            return new ValidationResult();
        }

        public ValidationResult Validate(ValidationContext context)
        {
            return new ValidationResult();
        }

        public IValidatorDescriptor CreateDescriptor()
        {
            return DummyValidatorDescriptor.Default;
        }

        public bool CanValidateInstancesOfType(Type type)
        {
            return true;
        }

        public Task<ValidationResult> ValidateAsync(ValidationContext context, CancellationToken cancellation = new CancellationToken())
        {
            return Task.FromResult(new ValidationResult());
        }

        public Task<ValidationResult> ValidateAsync(object instance, CancellationToken cancellation = new CancellationToken())
        {
            return Task.FromResult(new ValidationResult());
        }

        private class DummyValidatorDescriptor : IValidatorDescriptor
        {
            public static readonly DummyValidatorDescriptor Default = new DummyValidatorDescriptor();

            public string GetName(string property)
            {
                return property;
            }

            public ILookup<string, IPropertyValidator> GetMembersWithValidators()
            {
                return Enumerable.Empty<KeyValuePair<string, IPropertyValidator>>()
                    .ToLookup(p => p.Key, p => p.Value);
            }

            public IEnumerable<IPropertyValidator> GetValidatorsForMember(string name)
            {
                return Enumerable.Empty<IPropertyValidator>();
            }

            public IEnumerable<IValidationRule> GetRulesForMember(string name)
            {
                return Enumerable.Empty<IValidationRule>();
            }
        }
    }
}
