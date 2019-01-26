#include <iostream>
#include <vector>

using namespace std;


int main()
{
    bool verbose = false;
    // this application will test
    //    random range (i,j) for swaps
    //    based on byte entropy

    // array size for shuffle is n
    vector<pair<int,int>> summary_n(256, make_pair(-1,-1));

    // n >= 2 (n=1 has no pairs)
    for(int n=2; n<256; n++)
    {
        if(verbose)
            cout << "================== n:" << n << " ===================" << endl;
        vector<pair<int,int>> summary_i(n, make_pair(-1,-1));
        for(int i=0; i<n-1; i++)
        {
            int* count_j = new int[n];
            for(int j=0; j<n; j++)
                count_j[j] = 0;

            // verify all possible "random" values
            for(int random_byte = 0; random_byte < 256; random_byte++)
            {
                // swap pair is random_byte mod range
                int j = (random_byte % (n-i)) + i;
                count_j[j]++;
            }

            // compute summary (min,max) for i:
            int min_j = 256+1, max_j = -1;
            for(int j=i; j<n; j++)
            {
                if(count_j[j] < min_j)
                    min_j = count_j[j];
                if(count_j[j] > max_j)
                    max_j = count_j[j];
                if(verbose)
                    cout << "i=" << i << " j=" << j << " : " << count_j[j] << endl;
            }
            summary_i[i] = make_pair(min_j, max_j);
        } // finish i

        // compute summary (min,max) for i differences:
        int min_i_diff = 256+1, max_i_diff = -1;
        for(int i=0; i<n-1; i++)
        {
            int diff_i = summary_i[i].second - summary_i[i].first; // max - min
            if(verbose)
                cout << "n=" << n << " i=" << i << " diff = " << summary_i[i].second << " - " << summary_i[i].first << "=" << diff_i << endl;
            if(diff_i < min_i_diff)
                min_i_diff = diff_i;
            if(diff_i > max_i_diff)
                max_i_diff = diff_i;
        }
        summary_n[n] = make_pair(min_i_diff, max_i_diff);

        if(verbose)
            getchar();
    } // finish n

    cout << "final summary for n:" << endl;
    for(int n=2; n<256; n++)
    {
        cout << "n=" << n << " diff:" << summary_n[n].second - summary_n[n].first << " min:" << summary_n[n].first << " max:" << summary_n[n].second << endl;
    }

    // this application demonstrates that the maximum difference (bias) between element selection is 1
    // in other words, very little (minimum) bias is added by using small range byte to select random elements
    return 0;
}
