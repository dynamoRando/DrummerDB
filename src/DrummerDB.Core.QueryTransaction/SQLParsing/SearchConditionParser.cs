using Antlr4.Runtime;
using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;
using Drummersoft.DrummerDB.Core.Databases.Interface;
using Drummersoft.DrummerDB.Core.QueryTransaction.Enum;
using Drummersoft.DrummerDB.Core.QueryTransaction.Interface;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using static TSqlParser;

namespace Drummersoft.DrummerDB.Core.QueryTransaction.SQLParsing
{
    /// <summary>
    /// Used to evaluate search objects from Antlr in the <see cref="TSqlParser"/> namespace and provide needed changes
    /// to both a <see cref="IStatement"/> and it's later corresponding <see cref="QueryPlan"/>.
    /// </summary>
    class SearchConditionParser
    {
        #region Public Methods

        /// <summary>
        /// Evaulates the search context and applies appropriate changes to the supplied Statement
        /// </summary>
        /// <param name="context">The context to evaulate</param>
        /// <param name="statement">The statement to apply changes to</param>
        /// <param name="tokenStream">The token stream (for whitespace parsing)</param>
        /// <param name="stream">The char stream (for whitespace parsing)</param>
        public void EvaluateSearchCondition([NotNull] Search_conditionContext context, IWhereClause searchableStatement, CommonTokenStream tokenStream, ICharStream stream, QueryPlan plan, IDatabase db, string tableName)
        {
            string debug = context.GetText();
            var interval = context.SourceInterval;

            Debug.WriteLine("EvaluateSearchCondition:");
            Debug.WriteLine($"Text: {debug}");
            Debug.WriteLine($"Interval: {interval.ToString()}");

            if (!searchableStatement.HasWhereClause)
            {
                searchableStatement.WhereClause = new WhereClause();
                string fullText = ParserUtil.GetWhiteSpaceFromCurrentContext(context, stream);
                searchableStatement.WhereClause.FullText = fullText;
            }

            var predicate = context.predicate();
            if (predicate != null)
            {
                int searchContextCount = 0;
                if (context.Parent.ChildCount > 1)
                {
                    int childCount = context.Parent.ChildCount;
                    for (int a = 0; a < childCount; a++)
                    {
                        var child = context.Parent.GetChild(a);
                        if (child is Search_conditionContext)
                        {
                            searchContextCount++;
                        }
                    }
                }


                if (context.Parent.ChildCount == 1 || context.Parent is Query_specificationContext)
                {
                    EvaulateSinglePredicate(context, searchableStatement, tokenStream, stream, plan, db, tableName);
                }
                else if (searchContextCount == 1)
                {
                    EvaulateSinglePredicate(context, searchableStatement, tokenStream, stream, plan, db, tableName);
                }
                else
                {
                    EvaluateMultiplePredicates(context, searchableStatement, tokenStream, stream, plan, db, tableName);
                }
            }
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// Evaluates a WHERE clause for multiple predicates tied together by booleans and applies changes to the provided statement. This function will be recursively called.
        /// </summary>
        /// <param name="context">The search context to evaluate</param>
        /// <param name="statement">The statement to apply changes to</param>
        /// <param name="tokenStream">The token stream</param>
        /// <param name="stream">The char stream</param>
        private void EvaluateMultiplePredicates(Search_conditionContext context, IWhereClause searchableStatement, CommonTokenStream tokenStream, ICharStream stream, QueryPlan plan, IDatabase db, string tableName)
        {
            var predicate = context.predicate();
            if (predicate != null)
            {
                if (PredicateHasBool(context))
                {
                    WalkContext(context, searchableStatement, tokenStream, stream, tableName);
                }
            }
        }

        /// <summary>
        /// Evaulates a WHERE clause with a single predicate
        /// </summary>
        /// <param name="context">The search condition</param>
        /// <param name="statement">The statement to apply the predicate to</param>
        /// <param name="tokenStream">The token stream (for whitespace parsing)</param>
        /// <param name="stream">The char stream (for whitespace parsing)</param>
        private void EvaulateSinglePredicate(Search_conditionContext context, IWhereClause searchableStatement, CommonTokenStream tokenStream, ICharStream stream, QueryPlan plan, IDatabase db, string tableName)
        {
            var predicate = context.predicate();
            if (predicate is not null)
            {
                string whereClauseText = ParserUtil.GetWhiteSpaceFromCurrentContext(context, stream);
                var part = new Predicate(searchableStatement.GetMaxWhereClauseId(), whereClauseText, tableName);
                searchableStatement.WhereClause.Add(part);
            }
        }

        private BoolPredicate GetBooleanPredicate(Interval interval, WhereClause whereClause)
        {
            var foo = whereClause.GetPredicate(interval);
            if (foo is not null)
            {
                if (foo is BoolPredicate)
                {
                    return foo as BoolPredicate;
                }
            }

            return null;
        }

        /// <summary>
        /// Searches the provided list of parse tree items and returns the appropriate <see cref="BooleanComparisonOperator"/> if it finds
        /// any keyword
        /// </summary>
        /// <param name="items">The parse tree items to search</param>
        /// <returns>The boolean operator found</returns>
        private BooleanComparisonOperator GetBooleanComparison(IList<IParseTree> items)
        {
            foreach (var item in items)
            {
                var childText = item.GetText();
                if (childText == "AND")
                {
                    return BooleanComparisonOperator.And;
                }

                if (childText == "OR")
                {
                    return BooleanComparisonOperator.Or;
                }
            }

            return BooleanComparisonOperator.Unknown;
        }

        private void WalkContext(RuleContext context, IWhereClause searchableStatement, CommonTokenStream tokenStream, ICharStream stream, string tableName)
        {
            /*

             This function is pretty nuts. Will try to refactor later.

             Some definitions: 
             A predicate is a search condition, i.e "FOO = 1". 
             A bool predicate (bool operation) is two predicates (or another bool predicate) operated on by a BOOL operation (i.e. AND, OR).
             A bool predicate has a "left hand" and a "right hand" which can be either a predicate, or another bool predicate.
             This function always assumes that if bool predicates are in a chain, that the left hand of the bool predicate will be it's 
             child boolean operations.

             We are trying to evaluate a boolean chain, for example
             WHERE (FOO = A OR BAR > 2) AND BAZ = 'BUN' 
             or 
             WHERE ((FOO = A OR BAR > 2) AND BOO = TRUE) AND (BAZ = 'BUN')

             When we first get into the chain, we attempt to pull the first predicate - sometimes it's there, sometimes it's in the child.
             We then try to get the following predicate and then check to see if we've already got it as part of a BOOL operation.
             If yes, we need to tie it to that BOOL predicate. Otherwise, it may be part of a previous BOOL operation that we also need to reference -

             Example: 
             (FOO = A OR BAR > 2) AND BOO = TRUE

             This breaks down into -

             "(FOO = A OR BAR > 2)" as BOOL operation 1
             [ BOOL operation 1 ] AND "BOO = TRUE" as BOOL operation 2 

             For definitions, in the following statement -

             "FOO = A" OR "BAR > 2"

             "FOO = A" may be the predicate, and "BAR > 2" may be it's child (based on the way the parse tree works) depending on where we are
             in the parse tree. This entire function operates off of that assumption and tries to account for which part we are in.

             For example, on the next iteration of the function, if the clause is ((FOO = A OR BAR > 2) AND BOO = TRUE), then 

             "(FOO = A OR BAR > 2)"

             May be the previous BOOL operation predicate that we're searching for, and we need to point "BOO = TRUE" as the Right hand predicate
             of it.

             The rest of the function then tries to find if the predicate, or the child predicate is part of a boolean operation and tries to tie 
             either the predicate or the child predicate to the appropriate BOOL operation.

             */

            Predicate predicate = null;
            BooleanComparisonOperator boolValue = BooleanComparisonOperator.Unknown;

            if (context is TSqlParser.Search_conditionContext)
            {
                string debug = context.GetText();
                var searchContext = context as TSqlParser.Search_conditionContext;

                // get a predicate from the current search context
                var searchPredicate = searchContext.predicate();
                if (searchPredicate is not null)
                {
                    predicate = GetPredicate(searchPredicate, searchableStatement, stream, tableName);
                }
                else
                {
                    // search the children for the predicate
                    // this happens when the WHERE clause has multiple BOOLEANS and is seperated by parenthesis i.e. OR ( FIELD = 'A' )
                    // in other words the predicate is ( FIELD = 'A' )
                    // and so the searchContext does indeed have a predicate, it's just wrapped in ( )
                    foreach (var c in searchContext.children)
                    {
                        if (c is Search_conditionContext)
                        {
                            var x = c as Search_conditionContext;
                            var p = x.predicate();
                            if (p is not null)
                            {
                                predicate = GetPredicate(p, searchableStatement, stream, tableName);
                            }
                        }
                    }
                }
            }

            // if the predicate is part of a BOOLEAN chain 
            if (context.Parent.ChildCount > 0)
            {
                if (context.Parent is Search_conditionContext)
                {
                    var parent = context.Parent as Search_conditionContext;

                    bool anyChildrenHaveBool = parent.children.Any(child => string.Equals(child.GetText(), "AND", StringComparison.OrdinalIgnoreCase) || string.Equals(child.GetText(), "OR", StringComparison.OrdinalIgnoreCase));

                    if (anyChildrenHaveBool)
                    {
                        boolValue = GetBooleanComparison(parent.children);

                        foreach (var child in parent.children)
                        {
                            var text = child.GetText();
                            if (!string.Equals("AND", text, StringComparison.OrdinalIgnoreCase) && !string.Equals("OR", text, StringComparison.OrdinalIgnoreCase))
                            {
                                if (child is Search_conditionContext)
                                {
                                    var c = child as Search_conditionContext;
                                    var predicateChild = GetPredicate(c, searchableStatement, stream, tableName);

                                    if (predicateChild is null)
                                    {
                                        // check to see if the predicate is in a previous boolean predicate
                                        // we search for the previous boolean predicate by the interval location
                                        // but due to parenthsis location, we need to account for the shift in 
                                        // multiple predicates

                                        var childSourceInterval = child.SourceInterval;
                                        var childInterval = new Interval { A = childSourceInterval.a, B = childSourceInterval.b };

                                        var textCharacters = text.ToCharArray();
                                        foreach (var ch in textCharacters)
                                        {
                                            if (ch == Char.Parse("("))
                                            {
                                                childInterval.A += 1;
                                            }

                                            /*
                                            if (ch == Char.Parse(")"))
                                            {
                                                dChildInterval.B -= 1;
                                            }
                                            */
                                        }

                                        if (text.EndsWith(")"))
                                        {
                                            childInterval.B -= 1;
                                        }

                                        // we are searching for any predicate that has the complete interval
                                        // this may be a wide interval, i.e. (FOO = A OR BAR > 2) AND BOO = TRUE)
                                        // where there is an existing BOOL chain we're looking for
                                        if (searchableStatement.WhereClause.HasPredicate(childInterval))
                                        {
                                            /*
                                             * we need to point this predicate to the previous boolean interval
                                             * from the above comment example 
                                             * 
                                             * (FOO = A OR BAR > 2) as BOOL operation 1
                                             * [BOOL operation 1] AND BOO = TRUE as BOOL operation 2
                                             * 
                                             * In the below code, "booleanPredicate.Right" is "BOOL = TRUE" and is BOOL operation 2
                                             * and "booleanPredicate.Left" is BOOL operation 1
                                             * 
                                             */
                                            var existingBoolean = GetBooleanPredicate(childInterval, searchableStatement.WhereClause);
                                            if (existingBoolean is not null)
                                            {
                                                if (predicate is not null)
                                                {
                                                    var booleanPredicate = new BoolPredicate(searchableStatement.GetMaxWhereClauseId() + 1);
                                                    booleanPredicate.ComparisonOperator = boolValue;
                                                    booleanPredicate.Left = existingBoolean;
                                                    booleanPredicate.Right = predicate;
                                                    searchableStatement.WhereClause.Add(booleanPredicate);
                                                    return;
                                                }
                                            }
                                            else
                                            {
                                                return;
                                            }
                                        }
                                    }

                                    // we need to check to see if there is already a boolean in the bucket
                                    // for this predicate
                                    if (predicate is not null)
                                    {
                                        if (searchableStatement.WhereClause.HasBooleanPredicate(predicate))
                                        {
                                            var existingBoolean = searchableStatement.WhereClause.GetBooleanPredicate(predicate);
                                            existingBoolean.Right = predicateChild;
                                        }
                                        else
                                        {
                                            if (!searchableStatement.WhereClause.HasPredicate(predicate.Interval))
                                            {
                                                // this is a new boolean we need to add
                                                // i.e. FOO = A OR [ ___ ]
                                                // where the Right predicate will be filled out later
                                                // (see above statement)
                                                var booleanPredicate = new BoolPredicate(searchableStatement.GetMaxWhereClauseId() + 1);
                                                booleanPredicate.ComparisonOperator = boolValue;
                                                booleanPredicate.Left = predicate;
                                                searchableStatement.WhereClause.Add(booleanPredicate);
                                            }
                                        }

                                    }
                                }
                            }
                        }

                        return;
                    }

                    // if we don't have any bools, we need to scan the child parent just in case
                    // see if we're caught in between parenthesis
                    bool anyChildrenHaveParen = parent.children.Any(child => child.GetText().StartsWith("(") || child.GetText().StartsWith(""));
                    if (anyChildrenHaveParen)
                    {
                        // walk the children and skip any parenthesis
                        foreach (var child in parent.children)
                        {
                            var childText = child.GetText();
                            if (childText.StartsWith("(") || childText.StartsWith(")"))
                            {
                                continue;
                            }
                            else
                            {
                                // if we have a statement, and it is a search condition, then try and get the child context to recurse to get the last
                                // boolean operator
                                if (child is TSqlParser.Search_conditionContext)
                                {
                                    var recursiveChild = child as TSqlParser.Search_conditionContext;

                                    string debug = recursiveChild.Parent.GetText();
                                    string debugGrandParent = recursiveChild.Parent.Parent.GetText();

                                    var grandParent = recursiveChild.Parent.Parent;

                                    if (grandParent is TSqlParser.Search_conditionContext)
                                    {
                                        var gp = grandParent as TSqlParser.Search_conditionContext;
                                        foreach (var grandChild in gp.children)
                                        {
                                            if (grandChild is TSqlParser.Search_conditionContext)
                                            {
                                                var gc = grandChild as TSqlParser.Search_conditionContext;
                                                WalkContext(gc, searchableStatement, tokenStream, stream, tableName);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }


        /// <summary>
        /// Returns a <see cref="Predicate"/> from one of Antlr's <see cref="Search_conditionContext"/> objects
        /// </summary>
        /// <param name="context">The context</param>
        /// <param name="statement">The current SQL statement</param>
        /// <param name="stream">The current char stream</param>
        /// <returns>A predicate parsed from the provided context</returns>
        private Predicate GetPredicate(TSqlParser.Search_conditionContext context, IWhereClause searchableStatement, ICharStream stream, string tableName)
        {
            var predicate = context.predicate();

            if (predicate is not null)
            {
                return GetPredicate(predicate, searchableStatement, stream, tableName);
            }

            return null;
        }

        /// <summary>
        /// Returns a <see cref="Predicate"/> from one of Antlr's <see cref="PredicateContext"/> objects
        /// </summary>
        /// <param name="context">The context</param>
        /// <param name="statement">The current SQL statement</param>
        /// <param name="stream">The current char stream</param>
        /// <returns>A predicate parsed from the provided context</returns>
        private Predicate GetPredicate(TSqlParser.PredicateContext context, IWhereClause searchableStatement, ICharStream stream, string tableName)
        {
            var predicateInterval = context.SourceInterval;
            var bucketInterval = new Interval { A = predicateInterval.a, B = predicateInterval.b };

            string fullText = ParserUtil.GetWhiteSpaceFromCurrentContext(context, stream);

            var newPredicate = new Predicate(searchableStatement.GetMaxWhereClauseId() + 1, fullText, tableName);
            newPredicate.SetInterval(bucketInterval);

            return newPredicate;
        }

        /// <summary>
        /// Determines if the predicate is part of a bool chain
        /// </summary>
        /// <param name="context">The search context</param>
        /// <returns>True if the predicate is part of a bool chain, otherwise false</returns>
        private bool PredicateHasBool(RuleContext context)
        {
            if (context.Parent.ChildCount > 0)
            {
                if (context.Parent is TSqlParser.Search_conditionContext)
                {
                    var parent = context.Parent as TSqlParser.Search_conditionContext;

                    bool anyChildrenHaveBools = parent.children.Any(child => string.Equals(child.GetText(), "AND", StringComparison.OrdinalIgnoreCase) || string.Equals(child.GetText(), "OR", StringComparison.OrdinalIgnoreCase));

                    if (anyChildrenHaveBools)
                    {
                        return true;
                    }
                    else
                    {
                        // we need to check for the corner cases where it is actually part of a bool chain, but it's isolated from the others via a parenthsis
                        // we do this by checking the children's parent
                        bool anyChildrenHaveParen = parent.children.Any(child => child.GetText().StartsWith("(") || child.GetText().StartsWith(")"));

                        if (anyChildrenHaveParen)
                        {
                            foreach (var child in parent.children)
                            {
                                var childText = child.GetText();
                                if (childText.StartsWith("(") || childText.StartsWith(")"))
                                {
                                    continue;
                                }
                                else
                                {
                                    if (child is TSqlParser.Search_conditionContext)
                                    {
                                        var recursiveChild = child as TSqlParser.Search_conditionContext;
                                        return PredicateHasBool(recursiveChild.Parent);
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return false;
        }

        #endregion
    }
}


