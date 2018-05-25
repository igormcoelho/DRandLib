##AnonyTree - generating anonymous element from a publicly known list


### Basic idea
The idea of AnonyTree algorithm is to allow that a subset of a public list of N known 
elements to become anonymous, even on a fully decentralized and transparent platform.
The core assumption is that, to start from a public towards an anonymous list, it's
necessary that each element holds "a small portion of the secret", and so it's also
necessary to have incentives for the involved parties in not cheating or revealing
parts of the secret.

So, the algorithm does not demand any centralized authority to hold any secret, as
the secret will be gradually kept by elements in a sequential tournment.
This tournment starts from elements being shuffled (according to a known hash seed),
and although it could be implemented with bigger sets of competing elements, we will
focus on a binary tournment proposal (with some exceptions if N is not in form 2^k), 
since it maximizes the anonymity of the process, while slightly sacrifycing performance.
The tournment between elements will thus form a binary tournment tree, called AnonyTree.


