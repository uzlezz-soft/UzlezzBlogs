const commentForm = document.getElementById("commentForm");
const commentTextarea = document.getElementById("commentContent");
const commentList = document.getElementById("commentList");
const commentCount = document.getElementById("commentCount")

commentForm.addEventListener("submit", async (e) => {
    e.preventDefault();

    const originalContent = commentTextarea.value;
    const content = originalContent.trim();
    if (!content) return;
    
    commentTextarea.value = "";

    const response = await fetch(`/comment/${postId}?content=${encodeURIComponent(content)}`, {
        method: "POST",
        headers: {
            "RequestVerificationToken": document.querySelector('input[name="__RequestVerificationToken"]')?.value ?? ""
        }
    });

    if (response.ok) {
        commentCount.innerHTML = parseInt(commentCount.textContent) + 1;

        const result = await response.json();

        const commentData = {
            createdDate: new Date().toLocaleString(undefined, {
                day: 'numeric',
                month: 'long',
                year: 'numeric',
                hour: 'numeric',
                minute: '2-digit',
                hour12: prefers12Hour
            }),
            content: result.text,
            user: result.user
        };

        const commentHtml = `
        <div class="d-flex mb-4">
                <img src="/avatar/${commentData.user}"
                    alt="avatar"
                    class="rounded-circle me-3 comment-avatar"
                    style="width: 48px; height: 48px; object-fit: cover;"
                    profile="/profile/${commentData.user}" />

            <div class="flex-grow-1">
                <div class="d-flex justify-content-between align-items-center">
                    <strong>${commentData.user}</strong>
                    <small class="text-muted">${commentData.createdDate}</small>
                </div>
                <p class="mt-1 mb-0" style="white-space: pre-wrap;">${commentData.content}</p>
            </div>
        </div>
        `;

        commentList.insertAdjacentHTML("afterbegin", commentHtml);
        const el = commentList.querySelector(".comment-avatar")
        el.addEventListener("click", () => {
            const profile = el.getAttribute("profile");
            if (profile) window.location.href = profile;
        })
    } else {
        commentTextarea.value = originalContent;
        alert("Failed to post comment.");
    }
});