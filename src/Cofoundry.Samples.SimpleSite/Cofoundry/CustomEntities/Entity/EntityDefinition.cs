﻿using Cofoundry.Domain;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Cofoundry.Samples.SimpleSite
{
    /// <summary>
    /// Each custom entity requires a definition class which provides settings
    /// describing the entity and how it should behave.
    /// </summary>
    public class EntityDefinition
        : ICustomEntityDefinition<EntityDataModel>,
        IOrderableCustomEntityDefinition
    {
        /// <summary>
        /// This constant is a convention that allows us to reference this definition code 
        /// in other parts of the application (e.g. querying)
        /// </summary>
        public const string DefinitionCode = "CUSENT";

        /// <summary>
        /// Unique 6 letter code representing the entity (the convention is to use uppercase)
        /// </summary>
        public string CustomEntityDefinitionCode => DefinitionCode;

        /// <summary>
        /// Singlar name of the entity
        /// </summary>
        public string Name => "Entity";

        /// <summary>
        /// Plural name of the entity
        /// </summary>
        public string NamePlural => "Entites";

        /// <summary>
        /// A short description that shows up as a tooltip for the admin 
        /// module.
        /// </summary>
        public string Description => "Custom entity";

        /// <summary>
        /// Indicates whether the UrlSlug property should be treated
        /// as a unique property and be validated as such.
        /// </summary>
        public bool ForceUrlSlugUniqueness => true;

        /// <summary>
        /// Indicates whether the url slug should be autogenerated. If this
        /// is selected then the user will not be shown the UrlSlug property
        /// and it will be auto-generated based on the title.
        /// </summary>
        public bool AutoGenerateUrlSlug => true;

        /// <summary>
        /// Indicates whether this custom entity should always be published when 
        /// saved, provided the user has permissions to do so. Useful if this isn't
        /// the sort of entity that needs a draft state workflow
        /// </summary>
        public bool AutoPublish => true;

        /// <summary>
        /// Indicates whether the entities are partitioned by locale
        /// </summary>
        public bool HasLocale => false;

        /// <summary>
        /// We're implementing ICustomizedTermCustomEntityDefinition here 
        /// in order to change the term for "Title" to "Name", which makes
        /// more sense when creating authors in the admin panel.
        /// </summary>
        public Dictionary<string, string> CustomTerms => new Dictionary<string, string>()
        {
            { CustomizableCustomEntityTermKeys.Title, "Name" }
        };

        /// <summary>
        /// The sorting to apply by default when querying collections of custom 
        /// entities of this type. A query can specify a sort type to override 
        /// this value.
        /// </summary>
        public CustomEntityQuerySortType DefaultSortType => CustomEntityQuerySortType.PublishDate;

        /// <summary>
        /// The default sort direction to use when ordering with the
        /// default sort type.
        /// </summary>
        public SortDirection DefaultSortDirection => SortDirection.Default;

        public CustomEntityOrdering Ordering => CustomEntityOrdering.Full;
    }
}