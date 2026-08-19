namespace CyberErp.Hrms.Dom.Constants
{
    /// <summary>Identity of the subsystem this application IS, within the shared CyberERP catalogue.</summary>
    public static class Subsystems
    {
        /// <summary>
        /// This application's abbreviation in <c>Core.Subsystem</c> — the stable key, unlike Name
        /// (a renameable label) or Code (now a numeric ordinal, "004").
        /// </summary>
        public const string OwnAbbreviation = "HRMS";

        /// <summary>
        /// The prefix operation links carry in the shared catalogue: <c>/hrms/branch</c>.
        ///
        /// <para>One catalogue serves every subsystem and the same screen name recurs across them, so
        /// each link is namespaced by its owner. It is an addressing convention of the CATALOGUE, not
        /// part of any URL — each subsystem SPA is served at the root of its own origin. Strip it
        /// before comparing a stored link with anything else.</para>
        /// </summary>
        public static readonly string LinkNamespace = OwnAbbreviation.ToLowerInvariant() + "/";
    }
}
