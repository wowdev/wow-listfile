#include <lookup3.h>

#include <iostream>
#include <stdexcept>
#include <vector>
#include <iomanip>
#include <algorithm>
#include <cstdint>
#include <fstream>

namespace
{
  std::uint64_t hashlittle (std::string normalized)
  {
    std::transform (normalized.begin(), normalized.end(), normalized.begin(), [] (char c) { return c == '/' ? '\\' : std::toupper (c); });
    std::uint32_t hashed_low (0);
    std::uint32_t hashed_high (0);
    hashlittle2 (normalized.c_str(), normalized.size(), &hashed_high, &hashed_low);
    return std::uint64_t (hashed_low) | (std::uint64_t (hashed_high) << 32UL);
  }
}

int main (int, char**)
try
{
  std::string filename;
  while (std::getline (std::cin, filename))
  {
    std::cout << std::setfill ('0') << std::setw (16) << std::hex << hashlittle (filename) << "\n";
  }

  return 0;
}
catch (std::exception const& ex)
{
  std::cerr << "EX: " << ex.what() << "\n";
  return 1;
}