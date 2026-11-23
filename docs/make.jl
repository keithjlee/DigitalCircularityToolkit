# Build the DigitalCircularityToolkit documentation site (published at https://keithjlee.com/DigitalCircularityToolkit).
#
# Local build:
#   julia --project=docs -e 'using Pkg; Pkg.instantiate()'     # once per machine
#   julia --project=docs docs/make.jl
#   open docs/build/index.html
#
# Generated at build time (git-ignored, do not edit):
#   docs/src/components/*.md  from docs/reference/components.json + docs/reference/notes/<Name>.md
#   docs/src/release-notes.md from the "# Release Notes" section of README.md
# components.json and the icons in docs/src/assets/icons come from the plugin itself:
#   tools/rhino-profile/run.sh tools/rhino-profile/dump_reference.py      (re-run after changing components)

using Documenter
using JSON

const DOCS = @__DIR__
const ROOT = dirname(DOCS)
const SRC = joinpath(DOCS, "src")

# ---------------------------------------------------------------- component reference

const SUBCATEGORIES = ["Objects", "Sets", "Characterization", "Distance", "Matching", "Utilities"]
const SUBCATEGORY_INTRO = Dict(
    "Objects" => "Turn geometry into design objects: an abstraction (line, plane, box or sphere) in a local frame found by PCA. See [Design objects](../concepts/design-objects.md) and [Orientation](../concepts/orientation.md).",
    "Sets" => "Expand design objects by their `Quantity` into per-instance lists for feature vectors and matching. See [Inventories](../concepts/inventories.md).",
    "Characterization" => "Describe objects as numbers: feature vectors, abstraction scores and Fourier shape descriptors. See [Feature vectors and cost matrices](../concepts/cost-matrices.md) and [Shape descriptors](../concepts/shape-descriptors.md).",
    "Distance" => "Cost matrices between demand and supply feature vectors, and distances between curves or point sets. See [Feature vectors and cost matrices](../concepts/cost-matrices.md).",
    "Matching" => "Assign supply to demand at minimum total cost and place the matched objects. See [Matching](../concepts/matching.md).",
    "Utilities" => "Move and orient objects, project outlines, and normalize values.",
)
const EXPOSURE_ORDER = Dict("primary" => 1, "secondary" => 2, "tertiary" => 3)
# Obsolete components (hidden, kept for saved files) come last on every page.
sortkey(c) = (get(c, "obsolete", false) ? 99 : get(EXPOSURE_ORDER, c["exposure"], 9), shortname(c))

shortname(c) = replace(c["name"], " (DCT)" => "")
pageslug(sub) = lowercase(sub)

# Table cells: escape pipes and keep everything on one line.
cell(s) = replace(replace(strip(string(s)), "|" => "\\|"), r"\s*\n\s*" => " ")

function paramtable(io, params)
    println(io, "| Name | Nickname | Type | Access | Default | Description |")
    println(io, "|:-----|:---------|:-----|:-------|:--------|:------------|")
    for p in params
        default = isempty(p["default"]) ? "" : "`" * join(p["default"], ", ") * "`"
        opt = p["optional"] ? " (optional)" : ""
        println(io, "| `", cell(p["name"]), "` | ", cell(p["nick"]), " | ", cell(p["type"]), " | ", p["access"], opt,
                " | ", default, " | ", cell(p["description"]), " |")
    end
end

function componentpages(components)
    outdir = joinpath(SRC, "components")
    mkpath(outdir)
    missing_notes = String[]
    pages = Pair{String,String}[]
    for sub in SUBCATEGORIES
        comps = sort(filter(c -> c["subcategory"] == sub, components);
                     by = sortkey)
        isempty(comps) && continue
        file = "components/$(pageslug(sub)).md"
        open(joinpath(SRC, file), "w") do io
            println(io, "# ", sub, "\n")
            println(io, SUBCATEGORY_INTRO[sub], "\n")
            for c in comps
                name = shortname(c)
                println(io, "## [", name, "](@id comp-", name, ")\n")
                haskey(c, "icon") && println(io, "![", name, " icon](../", c["icon"], ")\n")
                println(io, "*", strip(c["description"]), "*\n")
                println(io, "Nickname: `", c["nick"], "`\n")
                notefile = joinpath(DOCS, "reference", "notes", name * ".md")
                if isfile(notefile)
                    println(io, strip(read(notefile, String)), "\n")
                else
                    push!(missing_notes, name)
                end
                # Parameter types (e.g. DesignObject) have a note but no inputs/outputs.
                haskey(c, "inputs") || continue
                if c["variable_inputs"]
                    println(io, "!!! note \"Variable inputs\"\n    Zoom in on the component to add or remove inputs with the ⊕/⊖ buttons. The table lists the inputs of a newly placed component.\n")
                end
                println(io, "**Inputs**\n")
                paramtable(io, c["inputs"])
                println(io, "\n**Outputs**\n")
                paramtable(io, c["outputs"])
                println(io)
            end
        end
        push!(pages, sub => file)
    end

    # Overview: every component with its one-line description.
    open(joinpath(outdir, "index.md"), "w") do io
        println(io, "# Component reference\n")
        println(io, "All components are in the **DigitalCircularityToolkit** tab, grouped by subcategory as in the ribbon. ",
                "Each entry explains what the component computes and lists its inputs and outputs, as defined in the plugin.\n")
        for sub in SUBCATEGORIES
            comps = sort(filter(c -> c["subcategory"] == sub, components);
                         by = sortkey)
            isempty(comps) && continue
            println(io, "## ", sub, "\n")
            println(io, "| | Component | Description |")
            println(io, "|:---|:----------|:------------|")   # Julia Markdown needs >= 3 dashes per column
            for c in comps
                name = shortname(c)
                icon = haskey(c, "icon") ? "![](../$(c["icon"]))" : ""
                label = get(c, "obsolete", false) ? "*(obsolete)* " : ""
                println(io, "| ", icon, " | [", name, "](@ref comp-", name, ") | ", label, cell(c["description"]), " |")
            end
            println(io)
        end
    end

    isempty(missing_notes) || @warn "Components without a note in docs/reference/notes/: $(join(missing_notes, ", "))"
    return ["Overview" => "components/index.md"; pages]
end

# ---------------------------------------------------------------- release notes from the README

function releasenotes()
    readme = read(joinpath(ROOT, "README.md"), String)
    m = match(r"\n# Release Notes\n(.*?)(?=\n# |\n!\[\]\(|\z)"s, readme)
    m === nothing && error("README.md has no '# Release Notes' section")
    open(joinpath(SRC, "release-notes.md"), "w") do io
        println(io, "# Release notes\n")
        println(io, "Downloads for every version are on the [GitHub releases page](https://github.com/keithjlee/DigitalCircularityToolkit/releases).\n")
        print(io, strip(m.captures[1]), "\n")
    end
end

# ---------------------------------------------------------------- build

components = JSON.parsefile(joinpath(DOCS, "reference", "components.json"))
reference = componentpages(components)
releasenotes()

# Plugin version from the csproj (used for the search inventory).
const VERSION_STRING = match(r"<Version>(.*?)</Version>", read(joinpath(ROOT, "src", "DCT.GH1", "DigitalCircularityToolkit.csproj"), String)).captures[1]

makedocs(
    sitename = "DigitalCircularityToolkit",
    authors = "Keith J. Lee",
    modules = Module[],
    remotes = nothing,
    format = Documenter.HTML(
        prettyurls = false,   # raw-HTML videos use relative asset paths; keep page URLs as <page>.html everywhere
        canonical = "https://keithjlee.com/DigitalCircularityToolkit",
        edit_link = nothing,
        repolink = "https://github.com/keithjlee/DigitalCircularityToolkit",
        inventory_version = VERSION_STRING,
        assets = ["assets/dct.css"],
        size_threshold = 400 * 2^10,
        size_threshold_warn = 250 * 2^10,
    ),
    pages = [
        "Home" => "index.md",
        "Getting started" => "getting-started.md",
        "Concepts" => [
            "Design objects" => "concepts/design-objects.md",
            "Orientation" => "concepts/orientation.md",
            "Inventories" => "concepts/inventories.md",
            "Feature vectors and cost matrices" => "concepts/cost-matrices.md",
            "Matching" => "concepts/matching.md",
            "Shape descriptors" => "concepts/shape-descriptors.md",
        ],
        "Workflows" => [
            "Linear members from stock" => "workflows/linear.md",
            "Panels from reclaimed plates" => "workflows/planar.md",
            "Matching by shape" => "workflows/shapes.md",
            "Scanned objects" => "workflows/scans.md",
        ],
        "Component reference" => reference,
        "Release notes" => "release-notes.md",
    ],
)

# Julia's Markdown leaves a malformed table as a paragraph of pipes without any warning. Fail instead.
for (dir, _, files) in walkdir(joinpath(DOCS, "build")), f in files
    endswith(f, ".html") || continue
    html = read(joinpath(dir, f), String)
    m = match(r"<p>([^<]*\|\s*:?-)", html)   # a paragraph that starts like a table header + separator row
    m === nothing || error("Unparsed Markdown table in $(relpath(joinpath(dir, f), DOCS)): $(m.captures[1])")
end

deploydocs(
    repo = "github.com/keithjlee/DigitalCircularityToolkit.git",
    devbranch = "master",
    push_preview = false,
)
