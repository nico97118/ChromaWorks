CATEGORY
{
    name = ChromaWorks
    icon = RDicon_science
    colour = #00BFFF
    type = PART
    parent = none

    SUBCATEGORIES
    {
        SUBCATEGORY
        {
            name = All ChromaWorks
            icon = RDicon_science
            FILTER
            {
                CHECK
                {
                    type = tag
                    value = Chroma
                }
            }
        }

        SUBCATEGORY
        {
            name = Chips
            icon = RDicon_generic
            FILTER
            {
                CHECK
                {
                    type = tag
                    value = chip
                }
            }
        }
    }
}