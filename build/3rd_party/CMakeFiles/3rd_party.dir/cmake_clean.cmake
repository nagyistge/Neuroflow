file(REMOVE_RECURSE
  "3rd_party.pdb"
  "3rd_party.lib"
)

# Per-language clean rules from dependency scanning.
foreach(lang)
  include(CMakeFiles/3rd_party.dir/cmake_clean_${lang}.cmake OPTIONAL)
endforeach()
