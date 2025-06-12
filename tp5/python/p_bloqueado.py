import os
import matplotlib.pyplot as plt

q_values = [2, 4, 6, 8, 10]
output_dir = "../bin/Debug/net8.0/q_vs_t/"

q_probs = []

for Q in q_values:
    blocked = 0
    unblocked = 0

    for run in range(1, 21):  # usá 10 o 20 según cuántas corridas tengas
        base_name = f"output-simple-Q{Q}-beeman-run-{run}"
        blocked_file = os.path.join(output_dir, base_name + "-b.txt")
        normal_file = os.path.join(output_dir, base_name + ".txt")

        if os.path.exists(blocked_file):
            blocked += 1
        elif os.path.exists(normal_file):
            unblocked += 1

    total = blocked + unblocked
    prob = blocked / total if total > 0 else 0
    q_probs.append(prob)

# 📊 Graficar
plt.figure(figsize=(8, 5))
bars = plt.bar(q_values, q_probs, color='orangered', edgecolor='black')

# Etiquetas de valor arriba de cada barra
for bar in bars:
    height = bar.get_height()
    plt.text(bar.get_x() + bar.get_width() / 2, height, f"{height:.2f}",
             ha='center', va='bottom', fontsize=14)

plt.ylim(0, 1.05)
plt.xlabel(r"$Q_{\mathrm{in}}$ [1/s]", fontsize=20)
plt.ylabel(r"$P_{\mathrm{bloqueo}}$", fontsize=20)
#plt.title("Probabilidad de bloqueo vs. $Q_{\\mathrm{in}}$", fontsize=18)
plt.xticks(q_values, fontsize=20)
plt.yticks([0, 0.25, 0.5, 0.75, 1.0], fontsize=20)
plt.grid(axis='y', linestyle='--', alpha=0.5)
plt.tight_layout()
plt.show()
