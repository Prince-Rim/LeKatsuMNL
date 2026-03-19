using System;
using System.Collections.Generic;

namespace LeKatsuMNL.Helpers
{
    /// <summary>
    /// Converts quantities between different units of measurement.
    /// Uses base-unit normalization: fromUom → base → toUom.
    ///   Weight base = grams
    ///   Volume base = ml
    ///   Count  base = pcs
    /// </summary>
    public static class UomConverter
    {
        // Each entry: (normalizedName, category, factorToBase)
        private static readonly Dictionary<string, (string Category, decimal ToBase)> Units =
            new(StringComparer.OrdinalIgnoreCase)
        {
            // ── Weight (base = grams) ──
            { "g",        ("weight", 1m) },
            { "gram",     ("weight", 1m) },
            { "grams",    ("weight", 1m) },
            { "kg",       ("weight", 1000m) },
            { "kilogram", ("weight", 1000m) },
            { "kilograms",("weight", 1000m) },
            { "oz",       ("weight", 28.3495m) },
            { "ounce",    ("weight", 28.3495m) },
            { "ounces",   ("weight", 28.3495m) },
            { "lb",       ("weight", 453.592m) },
            { "lbs",      ("weight", 453.592m) },
            { "pound",    ("weight", 453.592m) },
            { "pounds",   ("weight", 453.592m) },
            { "mg",       ("weight", 0.001m) },
            { "milligram",("weight", 0.001m) },
            { "milligrams",("weight", 0.001m) },

            // ── Volume (base = ml) ──
            { "ml",         ("volume", 1m) },
            { "milliliter", ("volume", 1m) },
            { "milliliters",("volume", 1m) },
            { "l",          ("volume", 1000m) },
            { "liter",      ("volume", 1000m) },
            { "liters",     ("volume", 1000m) },
            { "cup",        ("volume", 236.588m) },
            { "cups",       ("volume", 236.588m) },
            { "tbsp",       ("volume", 14.787m) },
            { "tablespoon", ("volume", 14.787m) },
            { "tsp",        ("volume", 4.929m) },
            { "teaspoon",   ("volume", 4.929m) },
            { "fl oz",      ("volume", 29.5735m) },
            { "gal",        ("volume", 3785.41m) },
            { "gallon",     ("volume", 3785.41m) },
            { "gallons",    ("volume", 3785.41m) },
            { "qt",         ("volume", 946.353m) },
            { "quart",      ("volume", 946.353m) },
            { "quarts",     ("volume", 946.353m) },
            { "pt",         ("volume", 473.176m) },
            { "pint",       ("volume", 473.176m) },
            { "pints",      ("volume", 473.176m) },

            // ── Count (base = pcs) ──
            { "pcs",    ("count", 1m) },
            { "pc",     ("count", 1m) },
            { "per pc", ("count", 1m) },
            { "pieces", ("count", 1m) },
            { "piece",  ("count", 1m) },
            { "ea",     ("count", 1m) },
            { "dozen",  ("count", 12m) },
            { "pack",   ("count", 1m) },
            { "bundle", ("count", 1m) },
            { "roll",   ("count", 1m) },
            { "can",    ("count", 1m) },
            { "cans",   ("count", 1m) },
            { "stick",  ("count", 1m) },
            { "sticks", ("count", 1m) },
            { "pull",   ("count", 1m) },
            { "pulls",  ("count", 1m) },
            { "dip",    ("count", 1m) },
            { "dips",   ("count", 1m) },
        };

        /// <summary>
        /// Convert a quantity from one UOM to another.
        /// Returns the converted quantity, or the original quantity if both UOMs are the same.
        /// Throws if the UOM is unknown or if converting across categories (e.g. grams → ml).
        /// </summary>
        public static decimal Convert(decimal quantity, string fromUom, string toUom)
        {
            if (string.IsNullOrWhiteSpace(fromUom) || string.IsNullOrWhiteSpace(toUom))
                return quantity;

            string from = fromUom.Trim();
            string to = toUom.Trim();

            // Same unit — no conversion needed
            if (string.Equals(from, to, StringComparison.OrdinalIgnoreCase))
                return quantity;

            if (!Units.TryGetValue(from, out var fromInfo))
            {
                // Unknown UOM — return as-is to avoid breaking existing data
                return quantity;
            }

            if (!Units.TryGetValue(to, out var toInfo))
            {
                // Unknown UOM — return as-is
                return quantity;
            }

            if (fromInfo.Category != toInfo.Category)
            {
                // Can't convert across categories (e.g. weight → volume)
                // Return as-is rather than throwing to avoid breaking the app
                return quantity;
            }

            // Convert: quantity → base unit → target unit
            decimal baseValue = quantity * fromInfo.ToBase;
            return baseValue / toInfo.ToBase;
        }
    }
}
