import matplotlib.pyplot as plt

# Datos: 100 puntos equidistantes
#p = [0,0.01,0.02,0.03,0.04,0.049999997,0.06,0.07,0.08,0.089999996,0.099999994,0.11,0.12,0.13,0.14,0.14999999,0.16,0.17,0.17999999,0.19,0.19999999,0.21,0.22,0.22999999,0.24,0.25,0.26,0.26999998,0.28,0.29,0.29999998,0.31,0.32,0.32999998,0.34,0.35,0.35999998,0.37,0.38,0.39,0.39999998,0.41,0.42,0.42999998,0.44,0.45,0.45999998,0.47,0.48,0.48999998,0.5,0.51,0.52,0.53,0.53999996,0.55,0.56,0.57,0.58,0.59,0.59999996,0.61,0.62,0.63,0.64,0.65,0.65999997,0.66999996,0.68,0.69,0.7,0.71,0.71999997,0.72999996,0.74,0.75,0.76,0.77,0.78,0.78999996,0.79999995,0.81,0.82,0.83,0.84,0.84999996,0.85999995,0.87,0.88,0.89,0.9,0.90999997,0.91999996,0.93,0.94,0.95,0.96,0.96999997,0.97999996,0.98999995]
#consensus = [0.0432,0.9799352,0.9593196,0.937416,0.9135528,0.885982,0.8525524,0.8032096,0.6828868,0.4700276,0.0979064,0.091354,0.0925556,0.061956,0.0547392,0.0453202,0.0415844,0.0419896,0.0377704,0.0372956,0.0342612,0.0312836,0.0334352,0.0283908,0.0294092,0.0267228,0.0251352,0.025508,0.0245444,0.0236196,0.0237542,0.023236,0.0223742,0.0210528,0.0208324,0.0214968,0.020836,0.0192614,0.0198284,0.019184,0.0185812,0.0186492,0.01773,0.0176524,0.0178396,0.0172872,0.0173156,0.0165836,0.0161764,0.0163368,0.0157904,0.0160486,0.015424,0.0150544,0.0150028,0.0152788,0.0149368,0.0143624,0.0139758,0.013965,0.013864,0.0137566,0.0136758,0.012788,0.0135036,0.013034,0.0127792,0.0122384,0.0123928,0.0122628,0.011984,0.011634,0.011546,0.01131,0.0114432,0.0111756,0.011048,0.0110384,0.0108448,0.0108352,0.010201,0.010424,0.0099596,0.0098308,0.0096248,0.0094912,0.0095504,0.0090692,0.008897,0.0085828,0.0085612,0.0081688,0.007901,0.0073852,0.0069504,0.0063646,0.0057576,0.0050248,0.0041176,0.0033164]
#susceptibility = [0,0.039339066,0.08493662,0.13485551,0.22500753,0.37431717,0.5531311,1.4261901,24.640635,76.426605,16.62273,11.732921,8.838698,4.8463974,3.9362338,3.412026,2.1837492,2.1863904,1.9208182,1.9381405,1.4633558,1.4403034,1.4005418,1.2060956,1.1384488,0.95830196,0.9295285,0.88375324,0.8510216,0.8000406,0.8378072,0.74176145,0.691281,0.6145554,0.6493501,0.6706485,0.6001488,0.5071521,0.5546137,0.54497045,0.4878467,0.47298795,0.45190892,0.4474744,0.43674693,0.43492815,0.42758256,0.38290134,0.37747493,0.37041557,0.37089506,0.37377745,0.33862817,0.3356894,0.30986354,0.33365127,0.31225118,0.296774,0.28781036,0.2742671,0.26861218,0.2600925,0.25582987,0.25395647,0.25585276,0.24610712,0.2371701,0.21482588,0.21262705,0.20961055,0.20335178,0.1986611,0.18402952,0.18763298,0.17726175,0.18283772,0.17829105,0.1784611,0.16911659,0.16744812,0.1447288,0.14889136,0.14221032,0.13418125,0.13485767,0.12666965,0.13504505,0.11673963,0.11531591,0.10732927,0.09596541,0.09505197,0.090491705,0.07756945,0.06757385,0.058608275,0.046224304,0.036096066,0.02473182,0.016522128]

# Datos: 200 puntos entre 0 y 0.2
#p = [0,0.001,0.002,0.003,0.004,0.0050000004,0.006,0.007,0.008,0.009000001,0.010000001,0.011000001,0.012,0.013,0.014,0.015000001,0.016,0.017,0.018000001,0.019000001,0.020000001,0.021000002,0.022000002,0.023000002,0.024,0.025,0.026,0.027,0.028,0.029000001,0.030000001,0.031000001,0.032,0.033,0.034,0.035,0.036000002,0.037,0.038000003,0.039,0.040000003,0.041,0.042000003,0.043,0.044000003,0.045,0.046000004,0.047000002,0.048,0.049000002,0.05,0.051000003,0.052,0.053000003,0.054,0.055000003,0.056,0.057000004,0.058000002,0.059000004,0.060000002,0.061000004,0.062000003,0.063,0.064,0.065000005,0.066,0.067,0.068,0.069000006,0.07,0.071,0.072000004,0.07300001,0.074,0.075,0.076000005,0.07700001,0.078,0.079,0.080000006,0.081,0.082,0.083000004,0.08400001,0.085,0.086,0.087000005,0.08800001,0.089,0.09,0.091000006,0.09200001,0.093,0.094000004,0.095000006,0.096,0.097,0.098000005,0.09900001,0.1,0.101,0.102000006,0.10300001,0.104,0.105000004,0.106000006,0.10700001,0.108,0.109000005,0.11000001,0.11100001,0.112,0.113000005,0.11400001,0.115,0.116000004,0.117000006,0.11800001,0.119,0.120000005,0.12100001,0.12200001,0.123,0.124000005,0.125,0.126,0.127,0.128,0.12900001,0.13000001,0.13100001,0.132,0.133,0.134,0.135,0.136,0.13700001,0.13800001,0.13900001,0.14,0.141,0.142,0.143,0.14400001,0.14500001,0.14600001,0.147,0.148,0.149,0.15,0.15100001,0.15200001,0.15300001,0.15400001,0.155,0.156,0.157,0.158,0.15900001,0.16000001,0.16100001,0.162,0.163,0.164,0.165,0.16600001,0.16700001,0.16800001,0.16900001,0.17,0.171,0.172,0.17300001,0.17400001,0.17500001,0.17600001,0.177,0.178,0.179,0.18,0.18100001,0.18200001,0.18300001,0.18400002,0.185,0.186,0.187,0.18800001,0.18900001,0.19000001,0.19100001,0.192,0.193,0.194,0.19500001,0.19600001,0.19700001,0.19800001,0.19900002]
#consensus = [0.0432,0.0148604,0.0089868,0.1355776,0.6107688,0.2421784,0.3528244,0.2813188,0.9838716,0.981894,0.9799352,0.977794,0.9757084,0.1265468,0.9715964,0.969572,0.2710264,0.1596784,0.2096844,0.9613784,0.9593196,0.177568,0.115222,0.2854124,0.9505268,0.9484252,0.946128,0.480212,0.9418148,0.9398336,0.937416,0.0909288,0.93279,0.0476324,0.92809397,0.0850476,0.2145296,0.9206084,0.918438,0.9163212,0.9135528,0.2596424,0.1661988,0.906146,0.9032636,0.183734,0.8981776,0.8956984,0.3187148,0.8899732,0.885982,0.8832672,0.8800592,0.0592424,0.873914,0.869062,0.8656296,0.8629524,0.8600724,0.855526,0.8525524,0.848838,0.8452632,0.839378,0.8349996,0.8329052,0.824948,0.8228728,0.8130268,0.8122852,0.8032096,0.8019716,0.7920508,0.7883698,0.7808592,0.7693376,0.7661064,0.7199428,0.7389928,0.7287368,0.6828868,0.6456904,0.69919163,0.6907212,0.651654,0.6655308,0.495862,0.5328852,0.4168072,0.2342636,0.4700276,0.2054812,0.5086656,0.4002552,0.2835856,0.3059426,0.2862692,0.3177732,0.1121852,0.1229608,0.0979064,0.151706,0.1001438,0.134196,0.1612128,0.0713888,0.0904764,0.110812,0.1124788,0.1084564,0.091354,0.0584952,0.0843624,0.0877432,0.0767564,0.0793148,0.112178,0.0659788,0.1021812,0.0746196,0.0925556,0.061952,0.0620348,0.0694324,0.0705408,0.0581076,0.0636196,0.0663876,0.067626,0.0487688,0.061956,0.0541132,0.0527976,0.0608764,0.066714,0.0564916,0.0695632,0.0594836,0.0653444,0.057112,0.0547392,0.0477012,0.0657808,0.0456984,0.0570536,0.0468876,0.0540212,0.0538532,0.051854,0.0436964,0.0469248,0.0478452,0.0496706,0.0425308,0.0524924,0.0447468,0.0473352,0.0471112,0.0456572,0.0389344,0.0415844,0.042702,0.0405376,0.0385696,0.0417112,0.0412958,0.0409756,0.04138,0.0434488,0.038132,0.0419896,0.0390464,0.0374412,0.0439928,0.0390372,0.037032,0.0405076,0.0379204,0.0382884,0.0406444,0.0377704,0.03695,0.0359208,0.0359532,0.0375712,0.0363976,0.0351328,0.036751,0.0332708,0.034766,0.0372956,0.0361484,0.0336804,0.0355064,0.032092,0.0377064,0.0367356,0.0350068,0.03087,0.0354316]
#susceptibility = [0,0.043170113,0.12471515,0.3507221,1.8633902,1.6831048,0.7984601,1.9175745,0.032037497,0.03606081,0.039339066,0.04425645,0.048279762,0.5487399,0.05543232,0.06094575,0.9948201,0.77879524,0.65307134,0.080019236,0.08493662,1.3181008,2.2333627,1.3638288,0.10564923,0.10833144,0.110566616,3.861077,0.12204051,0.12665987,0.13485551,4.516391,0.15214086,1.7653912,0.16897917,1.5291537,7.3054805,0.1963973,0.20071864,0.2065301,0.22500753,6.741099,4.928922,0.25838614,0.27641654,11.653257,0.29578805,0.3130734,2.9005482,0.33915043,0.37431717,0.36880374,0.38966537,5.7428656,0.4118681,0.47013164,0.4644692,0.45448542,0.47460198,0.55506825,0.5531311,0.58084726,0.6188452,0.7738173,0.8121133,0.770092,1.1147559,1.2004375,1.1783838,1.0545552,1.4261901,1.295507,1.3412535,1.2919307,1.745969,2.7385354,2.2892654,13.507009,5.153269,3.247857,24.640635,10.470897,4.481301,3.9523096,19.206106,8.135512,32.94196,18.718094,20.41731,12.881169,76.426605,28.201761,21.702126,20.10744,34.530087,50.70459,72.89111,59.341675,16.765387,22.715137,16.62273,34.472534,13.272334,19.227253,20.524426,7.097197,11.942556,17.652668,22.320347,9.574776,11.732921,4.383157,14.1198225,9.044898,7.309407,7.322747,19.652292,6.6274858,12.118039,5.3500533,8.838698,5.4070272,5.4354367,6.0881033,6.7225285,5.0563903,5.5504,6.0956974,6.5412045,3.5630987,4.8463974,3.68562,3.9230864,5.1475387,6.204557,5.4116244,6.9556923,3.8045697,5.1788635,5.3513346,3.9362338,3.299837,6.9189525,3.0717568,5.049877,3.6551716,3.9553864,4.7211065,3.8865674,2.7199912,3.4043367,3.3877056,3.2445853,2.594737,3.830326,2.7287245,2.884413,3.2700384,3.0931451,2.1110594,2.1837492,2.6049922,2.171724,2.1956131,2.442526,2.4011736,2.4821205,2.4510722,2.0844216,1.9024458,2.1863904,1.9967827,1.7616208,2.7029088,2.337619,2.000878,2.8983057,2.1114879,2.052409,2.4334123,1.9208182,1.6789863,1.7643443,1.8186349,2.177894,1.789725,1.7522415,2.0339632,1.634087,1.5983236,1.9381405,1.9142258,1.7353925,1.7105588,1.3356746,2.0207138,1.8412012,1.7208885,1.5428134,1.8479913]

# Datos: varios puntos entre 0 y 1 mayor distribución en p crítico
p = [0,0.01,0.02,0.03,0.04,0.05,0.06,0.07,0.08,0.085,0.09,0.095,0.1,0.105,0.15,0.2,0.3,0.4,0.5,0.6,0.7,0.8,0.9,1]
consensus = [0.0432,0.9799208,0.95953506,0.9377605,0.9140883,0.8872657,0.8532272,0.8053832,0.7332478,0.5450595,0.46026465,0.23031761,0.13804191,0.11647232,0.05521952,0.03440176,0.02351952,0.01867784,0.01620432,0.01423792,0.01198576,0.01046224,0.00836736,0]
susceptibility = [0,0.03784895,0.08061528,0.13291836,0.21263957,0.33065677,0.6316602,1.5448034,2.7444959,85.55814,79.85804,33.073452,19.813805,14.924696,3.5142143,1.6558322,0.82015276,0.5098346,0.36961195,0.28105018,0.20577759,0.15664472,0.10349431,0]


# Plot with discrete points (marker='o') connected by lines (-)
plt.plot(p, consensus, marker='o', linestyle='-', label="<M>", linewidth=0.8)
plt.ylabel("<M>")
plt.title("Promedio de consenso en estacionario")

#plt.plot(p, susceptibility, marker='o', linestyle='-', label="χ", linewidth=0.8)
#plt.ylabel("χ")
#plt.title("Promedio de susceptibilidad en estacionario")

# Labels and title
plt.xlabel("p")
#plt.legend()
plt.grid(True)
plt.tight_layout()
plt.show()
