#include <lookup3.h>

#include <algorithm>
#include <cstdint>
#include <fstream>
#include <iomanip>
#include <iostream>
#include <stdexcept>
#include <vector>

namespace
{
  std::uint64_t hashlittle (std::string str)
  {
    std::transform (str.begin(), str.end(), str.begin(), [] (char c) { return c == '/' ? '\\' : std::toupper (c); });
    std::uint32_t hashed_low (0);
    std::uint32_t hashed_high (0);
    hashlittle2 (str.c_str(), str.size(), &hashed_high, &hashed_low);
    return std::uint64_t (hashed_low) | (std::uint64_t (hashed_high) << 32UL);
  }
}

int main (int argc, char** argv)
try
{
  if (argc != 2)
  {
    throw std::invalid_argument ("missing argument: unknown hash text file");
  }

  std::vector<std::uint64_t> hashes;
  {
    std::ifstream hashfile (argv[1]);
    std::string line;
    while (std::getline (hashfile, line))
    {
      hashes.emplace_back (std::stoull (line, nullptr, 0x10));
    }
    std::sort (hashes.begin(), hashes.end());
  }

  std::string filename;
  while (std::getline (std::cin, filename))
  {
    if (std::binary_search (hashes.begin(), hashes.end(), hashlittle (filename)))
    {
      std::cout << filename << "\n";
    }
  }

  return 0;
}
catch (std::exception const& ex)
{
  std::cerr << "EX: " << ex.what() << "\n";
  return 1;
}