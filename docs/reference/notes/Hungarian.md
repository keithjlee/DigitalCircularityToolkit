!!! warning "Obsolete"
    Replaced first by [`Match`](@ref comp-Match) and now by [`Assign`](@ref comp-Assign). This component is hidden from the toolbar but still works in saved files.

Solves the same assignment problem as [`Match`](@ref comp-Match) on an integer `DistanceMatrix` tree, with the same inputs and outputs. Since v0.6 it uses the same in-house solver as [`Assign`](@ref comp-Assign) (it used the HungarianAlgorithm package before); the total cost is the same optimum, but among equally cheap assignments it may pick different pairs than before.
